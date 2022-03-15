using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace PingTicoVPN.Classes
{
    /// <summary>
    /// Represents a Network Interface for the purposes of this application.
    /// </summary>
    public class Interface
    {
        public int ifId { get; set; }                //Number of the Network Interface
        public string name { get; set; }             //Name of the Network Interface
        public IPAddress ipAddress { get; set; }     //IP Address of the Network Interface

    }
}
