using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace PingTicoVPNServer.Modules
{
    public static class Utils
    {
        public static bool ValidatePort(int port)
        {
            return port >= 1 && port <= 65535;
        }

        public static bool ValidateIpAddress(string ip_address)
        {
            Regex regex = new Regex(@"\b((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(\.|$)){4}\b");

            return String.IsNullOrEmpty(ip_address) ? false : regex.IsMatch(ip_address);
        }

        //Check if application pre-requisites
        public static void CheckPrerequisites(bool install = false)
        {
            Log.ToConsole(LogLevel.INFO, String.Format("Wireguard: {0}", File.Exists("/bin/wg")));
            if (!File.Exists("/bin/wg"))
            {
                if (install)
                {
                    Log.ToConsole(LogLevel.INFO, "Installing Wireguard...");
                    RunBash("apt install wireguard");  //Install Wireguard
                }
            }
            Log.ToConsole(LogLevel.INFO, String.Format("Wondershaper: {0}", File.Exists("/sbin/wondershaper")));
            if (!File.Exists("/sbin/wondershaper"))
            {
                if (install)
                {
                    Log.ToConsole(LogLevel.INFO, "Installing Wondershaper...");
                    RunBash("apt install wondershaper"); //Install Wondershaper
                }
            }
        }

        public static void RunBash(this string cmd)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = false,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            process.Start();
            process.WaitForExit();
        }
    }
}
