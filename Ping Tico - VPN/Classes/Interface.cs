using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace PingTicoVPN.Classes
{
    public class Interface
    {
        public int ifId { get; set; }
        public string name { get; set; }
        public IPAddress ipAddress { get; set; }

    }
}
