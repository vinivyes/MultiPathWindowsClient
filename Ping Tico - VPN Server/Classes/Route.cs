using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace PingTicoVPNServer.Classes
{
    /// <summary>
    /// Represents the connection between a Multi Path Client and a Multi Path Server
    /// </summary>
    class Bridge
    {

        public IPAddress ip { get; set; } //IP Address of the Bridge
        public int port { get; set; } //Port of the Bridge
        public DateTime registered_at { get; set; } //When was Bridge added ?


        private IPEndPoint remoteIpEndpoint; //The Bridge Endpoint 

        public Bridge(IPAddress _ip, int _port)
        {
            ip = _ip;
            port = _port;

            remoteIpEndpoint = new IPEndPoint(ip, port);

            registered_at = DateTime.Now;
        }

    }
}
