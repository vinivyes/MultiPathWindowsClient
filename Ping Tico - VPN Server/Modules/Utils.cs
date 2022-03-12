using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace PingTicoVPNServer.Modules
{
    public static class Utils
    {
        public static bool ValidatePort(int port)
        {
            return port >= 1 && port <= 65535;
        }

        public static bool ValidateIpAddress (string ip_address)
        {
            Regex regex = new Regex(@"\b((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(\.|$)){4}\b");

            return String.IsNullOrEmpty(ip_address) ? false : regex.IsMatch(ip_address);
        }
    }
}
