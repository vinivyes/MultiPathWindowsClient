using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace PingTicoVPNServer.Classes
{
    class Route
    {

        public IPAddress ip { get; set; } //IP Address of the Route
        public int port { get; set; } //Port of the Route
        public DateTime registered_at { get; set; } //When was route added ?


        private IPEndPoint RemoteIpEndpoint;

        public Route(IPAddress _ip, int _port)
        {
            ip = _ip;
            port = _port;

            RemoteIpEndpoint = new IPEndPoint(ip, port);

            registered_at = DateTime.Now;
        }

    }
}
