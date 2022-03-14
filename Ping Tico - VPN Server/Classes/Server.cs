using System;
using System.Collections.Generic;
using System.Text;

namespace PingTicoVPNServer.Modules
{
    /// <summary>
    /// Represents one server running an instance of this application. 
    /// Facilitates the communication between servers.
    /// </summary>
    public class Server
    {
        public string name { get; set; } //Server Name
        public string ipAddress { get; set; } //IP Address to reach the server
    }
}
