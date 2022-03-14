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

namespace PingTicoVPNServer.Modules
{
    /// <summary>
    /// This instance will receive UDP packets from any address, add said address to a list and forward responses back to the sender.
    /// No security features are implemented as of yet.
    /// No de-duplication
    /// </summary>
    
    public class MultiPathConnection
    {
        private UdpClient server; //Server used to receive/send data from the bridges

        public int wireguardPort { get; set; } = 31314;               //What port Wireguard is listening on
        public int serverPort { get; set; } = 31315;                  //What port client bridges need to send packets to
        public string wireguardAddress { get; set; } = "127.0.0.2";   //What address is Wireguard running on

        private bool mpActive = false; //If background tasks for MP should be running.

        //Will use default values
        public MultiPathConnection()
        {
        }

        //Load parameters and starts server
        public MultiPathConnection(string _WireguardAddress, int _WireguardPort, int _ServerPort)
        {
            wireguardAddress = _WireguardAddress;
            wireguardPort = _WireguardPort;
            serverPort = _ServerPort;
        }

        private Task mpBridges = null;
        private Task mpServer = null;
        private Task mpBridgesKeepAlive = null;

        private Dictionary<IPEndPoint, Bridge> bridges = new Dictionary<IPEndPoint, Bridge>();

        private IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);    //Which endpoint sent the latest packet
        private IPEndPoint wireguardIpEndpoint = null;

        //Socket to forward packets to Wireguard
        private Socket wireguardSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
            ProtocolType.Udp);

        public void StartMultiPath()
        {
            StopMultiPath();                    //Makes sure no background tasks are running.

            Log.ToConsole(LogLevel.INFO, String.Format("Starting MP Server for: *:{0} -> {1}:{2} ...", serverPort, wireguardAddress, wireguardPort));

            try
            {
                wireguardIpEndpoint = new IPEndPoint(IPAddress.Parse(wireguardAddress), wireguardPort);

                server = new UdpClient(serverPort); //Opens listen port on desired port

                wireguardSocket.Connect(wireguardIpEndpoint); //Connect to Wireguard

                Log.ToConsole(LogLevel.INFO, String.Format("Connected to Wireguard on {0}:{1}", wireguardAddress, wireguardPort));
            
                //Receives packets from Bridges and register new bridges
                mpBridges = Task.Run(() => {
                    while (mpActive)
                    {
                        try
                        {
                            Byte[] receiveBytes = server.Receive(ref remoteIpEndPoint);

                            wireguardSocket.Send(receiveBytes); //Forward packet to bridge

                            //Register new Bridges
                            if (!bridges.ContainsKey(remoteIpEndPoint))
                            {
                                Log.ToConsole(LogLevel.DEBUG,String.Format("[MultiPath - (*:{0}) -> ({1}:{2})] - Registering new Bridge {3}:{4}", serverPort, wireguardAddress, wireguardPort, remoteIpEndPoint.Address, remoteIpEndPoint.Port));

                                Bridge r = new Bridge(remoteIpEndPoint.Address, remoteIpEndPoint.Port);

                                bridges.Add(remoteIpEndPoint, r);
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
                            int readBytes = wireguardSocket.Receive(receivedBytes, SocketFlags.None);

                            //Crop buffer to received bytes only
                            byte[] cropBytes = new byte[readBytes];
                            for (int b = 0; b < readBytes; b++)
                            {
                                cropBytes[b] = receivedBytes[b];
                            }

                            //Forwards packets back through all bridges
                            foreach (KeyValuePair<IPEndPoint, Bridge> r in bridges)
                            {
                                server.Send(cropBytes, cropBytes.Length, r.Key);
                            }
                        }
                        catch {}
                    }
                });

                //Remove Bridges based on timeout value.
                mpBridgesKeepAlive = Task.Run(() =>
                {
                    while(mpActive)
                    {
                        //Wait before checking again. Ensures bridges are not disconnected all at once
                        Thread.Sleep(Config.config.mp_bridge_timeout_interval_sec * 1000);
                        foreach (KeyValuePair<IPEndPoint, Bridge> r in bridges)
                        {
                            if(DateTime.Now.Subtract(r.Value.registered_at).TotalMinutes > Config.config.mp_bridge_timeout_min)
                            {
                                bridges.Remove(r.Key);
                                break;
                            }
                        }
                    }
                });
            }
            catch (Exception ex) //In case of any exception stop mp server and log
            {
                StopMultiPath();
                Log.ToConsole(LogLevel.ERROR, String.Format("Unable to start MP Server for: {0} -> {1}:{2}", serverPort, wireguardAddress, wireguardPort));
                Log.HandleError(ex);
            }

            Log.ToConsole(LogLevel.INFO, String.Format("Started MP Server for: *:{0} -> {1}:{2}", serverPort, wireguardAddress, wireguardPort));
        }

        //Stops all background tasks and ensures the MP Server is ready to be restarted.
        public void StopMultiPath() {

            mpActive = false;

            //Dispose all background tasks
            if (mpBridges != null) { mpBridges.Dispose(); }
            if (mpServer != null) { mpServer.Dispose(); }
            if (mpBridgesKeepAlive != null) { mpBridgesKeepAlive.Dispose(); }
        }
    }
}
