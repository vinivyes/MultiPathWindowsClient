using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using PingTicoVPNServer.Classes;
using System.Runtime.Serialization;
using Utf8Json;

namespace PingTicoVPNServer.Modules
{
    /// <summary>
    /// This instance will receive UDP packets from any address, add said address to a list and forward responses back to the sender.
    /// No security features are implemented as of yet.
    /// </summary>
    
    public class MultiPathServer
    {
        [IgnoreDataMember]
        private UdpClient server; //Server used to receive/send data from the bridges

        public int WireguardPort { get; set; }  //What port Wireguard is listening on
        public int ServerPort { get; set; }     //What port client bridges need to send packets to

        public string WireguardAddress { get; set; }   //What address is Wireguard running on

        [IgnoreDataMember]
        public bool mpActive = false; //If background tasks for MP should be running.

        //Load parameters and starts server
        [SerializationConstructor]
        public MultiPathServer(string _WireguardAddress, int _WireguardPort, int _ServerPort)
        {
            WireguardAddress = _WireguardAddress;
            WireguardPort = _WireguardPort;
            ServerPort = _ServerPort;

            WireguardIpEndpoint = new IPEndPoint(IPAddress.Parse(WireguardAddress), WireguardPort);

            StartMultiPath();
        }

        [IgnoreDataMember]
        Task mpBridges = null;
        [IgnoreDataMember]
        Task mpServer = null;
        [IgnoreDataMember]
        Task mpRoutesKeepAlive = null;

        [IgnoreDataMember]
        private Dictionary<IPEndPoint, Route> routes = new Dictionary<IPEndPoint, Route>();

        [IgnoreDataMember]
        private IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);    //Which endpoint sent the latest packet
        [IgnoreDataMember]
        private IPEndPoint WireguardIpEndpoint = null;

        //Socket to forward packets to Wireguard
        [IgnoreDataMember]
        Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
            ProtocolType.Udp);

        public void StartMultiPath()
        {
            StopMultiPath();                    //Makes sure no background tasks are running.

            Log.LogToConsole(LogLevel.INFO, String.Format("Starting MP Server for: *:{0} -> {1}:{2} ...", ServerPort, WireguardAddress, WireguardPort));

            try
            {

                server = new UdpClient(ServerPort); //Opens listen port on desired port

                sock.Connect(WireguardIpEndpoint); //Connect to Wireguard

                Log.LogToConsole(LogLevel.INFO, String.Format("Connected to Wireguard on {0}:{1}", WireguardAddress, WireguardPort));
            
                //Receives packets from Bridges and register new bridges
                mpBridges = Task.Run(() => {
                    while (mpActive)
                    {
                        try
                        {
                            Byte[] receiveBytes = server.Receive(ref RemoteIpEndPoint);

                            sock.Send(receiveBytes); //Forward packet to bridge

                            //Register new Bridges
                            if (!routes.ContainsKey(RemoteIpEndPoint))
                            {
                                Log.LogToConsole(LogLevel.DEBUG,String.Format("[MultiPath - (*:{0}) -> ({1}:{2})] - Registering new Bridge {3}:{4}", ServerPort, WireguardAddress, WireguardPort, RemoteIpEndPoint.Address,RemoteIpEndPoint.Port));

                                Route r = new Route(RemoteIpEndPoint.Address, RemoteIpEndPoint.Port);

                                routes.Add(RemoteIpEndPoint, r);
                            }
                        }
                        catch {}
                    }
                });

                //Receives information back from Wireguard and forwards to all active bridges.
                mpServer = Task.Run(() => {
                    while (mpActive)
                    {
                        try
                        {
                            byte[] receivedBytes = new byte[4098];
                            int readBytes = sock.Receive(receivedBytes, SocketFlags.None);

                            //Crop buffer to received bytes only
                            byte[] cropBytes = new byte[readBytes];
                            for (int b = 0; b < readBytes; b++)
                            {
                                cropBytes[b] = receivedBytes[b];
                            }

                            //Forwards packets back through all bridges
                            foreach (KeyValuePair<IPEndPoint, Route> r in routes)
                            {
                                server.Send(cropBytes, cropBytes.Length, r.Key);
                            }
                        }
                        catch {}
                    }
                });

                //Remove routes based on timeout value.
                mpRoutesKeepAlive = Task.Run(() =>
                {
                    while(mpActive)
                    {
                        //Wait before checking again. Ensures bridges are not disconnected all at once
                        Thread.Sleep(Config.config.mp_bridge_timeout_interval_sec * 1000);
                        foreach (KeyValuePair<IPEndPoint, Route> r in routes)
                        {
                            if(DateTime.Now.Subtract(r.Value.registered_at).TotalMinutes > Config.config.mp_bridge_timeout_min)
                            {
                                routes.Remove(r.Key);
                                break;
                            }
                        }
                    }
                });
            }
            catch (Exception ex) //In case of any exception stop mp server and log
            {
                StopMultiPath();
                Log.LogToConsole(LogLevel.ERROR, String.Format("Unable to start MP Server for: {0} -> {1}:{2}", ServerPort, WireguardAddress, WireguardPort));
                Log.HandleError(ex);
            }

            Log.LogToConsole(LogLevel.INFO, String.Format("Started MP Server for: *:{0} -> {1}:{2}", ServerPort, WireguardAddress, WireguardPort));
        }

        //Stops all background tasks and ensures the MP Server is ready to be restarted.
        public void StopMultiPath() {

            mpActive = false;

            //Dispose all background tasks
            if (mpBridges != null) { mpBridges.Dispose(); }
            if (mpServer != null) { mpServer.Dispose(); }
            if (mpRoutesKeepAlive != null) { mpRoutesKeepAlive.Dispose(); }
        }
    }
}
