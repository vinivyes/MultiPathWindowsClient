using PingTicoVPNServer.Classes;
using PingTicoVPNServer.Modules;
using System;
using System.Collections.Generic;

namespace PingTicoVPNServer
{
    class Program
    {

        static void Main(string[] args)
        {
            Config.LoadConfig();
            LoadArguments(new string[] { "--new-mp-server" });
        }
    
        //Loads all arguments
        private static void LoadArguments(string[] a)
        {
            for(int i = 0; i < a.Length; i++)
            {
                if(a[i].ToLower() == "--new-mp-server")
                {
                    int retries = 0;

                    int ServerPort = -1;
                    
                    while (retries < 3 && !Utils.ValidatePort(ServerPort))
                    {
                        Console.WriteLine("Enter the Port the MP Server should listen at:");
                        int.TryParse(Console.ReadLine(), out ServerPort);
                        if(!Utils.ValidatePort(ServerPort))
                        {
                            retries++;
                            Console.WriteLine("{0}/3 - Invalid Server Port", retries);
                            if (retries > 3) { Environment.Exit(2); }
                        }
                    }
                    retries = 0;

                    string WireguardAddress = null;
                    while (retries < 3 && !Utils.ValidateIpAddress(WireguardAddress))
                    {
                        Console.WriteLine("Enter the address of Wireguard (xxx.xxx.xxx.xxx):");
                        WireguardAddress = Console.ReadLine();
                        if (!Utils.ValidateIpAddress(WireguardAddress))
                        {
                            retries++;
                            Console.WriteLine("{0}/3 - Invalid Wireguard Address", retries);
                            if (retries > 3) { Environment.Exit(3); }
                        }
                    }
                    retries = 0;

                    
                    int WireguardPort = -1;
                    while(retries < 3 && (!Utils.ValidatePort(WireguardPort) || ServerPort == WireguardPort))
                    {
                        Console.WriteLine("Enter the port of Wireguard:");
                        int.TryParse(Console.ReadLine(), out WireguardPort);
                        if (!Utils.ValidatePort(WireguardPort) || ServerPort == WireguardPort)
                        {
                            retries++;
                            Console.WriteLine("{0}/3 - Invalid Wireguard Port{1}", retries, ServerPort == WireguardPort ? " - Same port as Listen." : "");
                            if (retries > 3) { Environment.Exit(4); }
                        }
                    }

                    Config.config.mp_servers.Add(new MultiPathServer(WireguardAddress, WireguardPort, ServerPort));
                    Config.SaveConfig();
                }
            }
        }
    }
}
