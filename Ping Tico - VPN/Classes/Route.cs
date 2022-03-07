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

namespace PingTicoVPN.Classes
{
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

        //Sets the current selected status
        public void SetActiveStatus(bool a)
        {
            active = a;
            if (a)
            {
                try
                {
                    IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 0);

                    sock = new UdpClient();
                    sock.ExclusiveAddressUse = false;
                    sock.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    sock.Client.Bind(localEndPoint);
                    
                    StartReceivingDataFromServer();

                    Trace.WriteLine("Socket sending to " + ip + ":" + port + " is now Active and connected");
                }
                catch {
                    Trace.WriteLine("Socket sending to " + ip + ":" + port + " failed to connect/start");
                }
            }
            else
            {
                receivingData = false;
                receiveDataThread.Interrupt();

                sock.Close();
                sock.Dispose();
                sock = null;

                Trace.WriteLine("Socket sending to " + ip + ":" + port + " is now Disconnected");
            }
        }

        //Inverts Selection Status
        public void SwitchActiveStatus()
        {
            SetActiveStatus(!active);
        }

        private UdpClient sock;
        private IPEndPoint RemoteIpEndpoint;

        public Route(IPAddress _ip, int _port)
        {
            ip = _ip;
            port = _port;

            RemoteIpEndpoint = new IPEndPoint(ip, port);
        }

        //Replicates a packet to the server using this route
        public void ReplicatePacket(Byte[] data)
        {
            sock.Send(data, data.Length, RemoteIpEndpoint);
        }

        private static Thread receiveDataThread; //Thread that will receive data.
        private static bool receivingData = false; //Is currently receiving data.

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
            receivingData = true;
            while (receivingData)
            {
                try
                {

                    IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
                    byte[] receivedBytes = sock.Receive(ref remote);

                    MultiPath.SendDataBackToWireguard(receivedBytes);
                }
                catch { }
            }
        }
    }
}
