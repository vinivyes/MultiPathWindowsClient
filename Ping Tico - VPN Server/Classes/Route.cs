using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Ping_Tico___VPN_Server.Classes
{
    class Route
    {

        public IPAddress ip { get; set; } //IP Address of the Route
        public int port { get; set; } //Port of the Route
        public bool active { get; set; } //Is route active ?

        //Sets the current selected status
        public void SetActiveStatus(bool a)
        {
            active = a;
            if (a)
            {
                try
                {
                    IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 0);

                    Console.WriteLine("Socket sending to " + ip + ":" + port + " is now Active and connected");
                }
                catch
                {
                    Console.WriteLine("Socket sending to " + ip + ":" + port + " failed to connect/start");
                }
            }
            else
            {

                Console.WriteLine("Socket sending to " + ip + ":" + port + " is now deactivated");
            }
        }

        //Inverts Selection Status
        public void SwitchActiveStatus()
        {
            SetActiveStatus(!active);
        }

        private IPEndPoint RemoteIpEndpoint;

        public Route(IPAddress _ip, int _port)
        {
            ip = _ip;
            port = _port;

            RemoteIpEndpoint = new IPEndPoint(ip, port);
        }

    }
}
