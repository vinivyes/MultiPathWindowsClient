using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Threading;
using PingTicoVPN.Modules;
using System.Collections.ObjectModel;

namespace PingTicoVPN.Classes
{
    /// <summary>
    /// Represents a connection route to the server, may be bound to any network interface.
    /// </summary>
    public class Route
    {
        public string name { get; set; } //Name of the Route
        public IPAddress ip { get; set; } //IP Address of the Route
        public int port { get; set; } //Port of the Route

        private bool _active;
        public bool active { 
            get => _active; 
            set {
                _active = value;

                Active_Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                Inactive_Visibility = value ? Visibility.Collapsed : Visibility.Visible;

                Item_Color = value ? new SolidColorBrush(Color.FromRgb(255, 255, 255)) : new SolidColorBrush(Color.FromRgb(155, 155, 155));

            } 
        } //Is route active ?

        public Visibility Active_Visibility { get; set; } = Visibility.Collapsed; //If the UI should display this route as active
        public Visibility Inactive_Visibility { get; set; } = Visibility.Visible; //If the UI should display this route as inactive

        public SolidColorBrush Item_Color { get; set; } = new SolidColorBrush(Color.FromRgb(155, 155, 155)); //If the UI should display this route as inactive

        private Stopwatch pingSW = null;                                      //Used to measure route RTT

        public ObservableCollection<Interface> InterfaceList { get; set; }    //Reference to a list of interfaces.

        public string pingText { get; set; } = "- ms";                        //Result of latest ping in text

        private IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 0);  //Where to listen/bind the route. By default listens on all interfaces

        private bool blockRoute = false; //Blocks any changes to the status of the route unless change is force by parameter


        //Sets the current selected status, if blockRoute is true this will not run.
        public void SetActiveStatus(bool a, bool ignoreBlock = false)
        {
            if (blockRoute && !ignoreBlock) return;

            active = a;
            if (a)
            {
                try
                {

                    //Creates a new UDP Client bound to selected interface.
                    sock = new UdpClient();
                    sock.ExclusiveAddressUse = false;
                    sock.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    sock.Client.Bind(localEndPoint);
                    
                    StartReceivingDataFromServer();  //Runs thread to receive data from server

                    Log.ToConsole(LogLevel.INFO, String.Format("Socket sending to {0}:{1} is now Active and connected from {2}",ip, port, sock.Client.LocalEndPoint));
                }
                catch (Exception ex) {
                    Log.ToConsole(LogLevel.ERROR, String.Format("Socket sending to {0}:{1} failed to connect/start", ip, port));
                    Log.HandleError(ex);
                    active = false;
                }
            }
            else
            {
                ReplicatePacket(new byte[] { 1, 2, 3 });  //Send 3 byte packet signaling server to disconnect bridge.

                //Stop threads receving data from server
                receivingData = false;
                receiveDataThread.Interrupt();

                //Close and Dispose of socket.
                sock.Close();
                sock.Dispose();
                sock = null;

                Log.ToConsole(LogLevel.ERROR, String.Format("Socket sending to {0}:{1} is now Disconnected", ip, port));
            }
        }

        //Inverts Selection Status
        public void SwitchActiveStatus()
        {
            SetActiveStatus(!active);
        }

        private UdpClient sock;
        private IPEndPoint RemoteIpEndpoint;

        //Build remote IP Endpoint
        public Route(IPAddress _ip, int _port)
        {
            ip = _ip;
            port = _port;

            RemoteIpEndpoint = new IPEndPoint(ip, port);
        }

        //Recreates the route with the desired interface configuration.
        public void ReconfigureSocket(Interface i)
        {
            blockRoute = true; //Temporarilly prevents changes to the status of this route.

            bool initialStatus = active;   //Saves initial state of the route to return route to initial state later
            
            if(active) { SetActiveStatus(false, true); } //Disables route during changes if running

            localEndPoint = new IPEndPoint(i.ipAddress, 0); //Binds to selected interface

            if (initialStatus) { SetActiveStatus(true, true); } //Activates the route if it is activated on the first place

            blockRoute = false; //Disables temporary block to changes to the status of this route.

        }

        //Replicates a packet to the server using this route
        public void ReplicatePacket(Byte[] data)
        {
            sock.Send(data, data.Length, RemoteIpEndpoint);
        }

        //Replicates a packet to the server using this route
        public void UpdatePing()
        {
            if (active)
            {
                //Initiates and starts Stopwatch to measure RTT
                pingSW = new Stopwatch();
                pingSW.Start();

                ReplicatePacket(new byte[] { 1, 2 }); //Sends a 2 byte packet to server. 2 byte packets are sent back always.

            }
            
        }

        private Thread receiveDataThread; //Thread that will receive data.
        private bool receivingData = false; //Is currently receiving data.

        //Receive a packet from the server using this route
        public void StartReceivingDataFromServer()
        {
            //Clears any currently running thread
            if (receiveDataThread != null)
            {
                if (receiveDataThread.IsAlive)
                {
                    return;
                }
                try
                {
                    receivingData = false;
                    receiveDataThread.Interrupt();
                }
                catch { }
            }

            //Creates a new Thread
            receiveDataThread = new Thread(
                    new ThreadStart(ReceiveDataFromServer)
                );

            //Starts the new Thread
            receiveDataThread.Start();
        }

        //Receive a packet from the server using this route
        public void ReceiveDataFromServer()
        {
            receivingData = true; //Stops the while loop
            while (receivingData)
            {
                try
                {

                    IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0); //Where data came from
                    byte[] receivedBytes = sock.Receive(ref remote);      //Receives data

                    //If received a 2 byte packet, this is a response from a ping. Update latency information
                    if (receivedBytes.Length == 2)
                    {
                        if(pingSW != null) { 
                            pingSW.Stop();
                            pingText = String.Format("{0} ms",pingSW.Elapsed.TotalMilliseconds.ToString("0.##"));

                        }
                        continue;
                    }

                    MultiPath.SendDataBackToWireguard(receivedBytes);
                }
                catch (Exception ex){
                    Log.HandleError(ex);
                }
            }
        }
    }
}
