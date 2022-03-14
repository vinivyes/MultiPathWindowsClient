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
            //LoadArguments(new string[] { "--new-mp-server" });
        }
    
        //Loads all arguments
        private static void LoadArguments(string[] a)
        {
            for(int i = 0; i < a.Length; i++)
            {
                if(a[i].ToLower() == "--new-mp-server")
                {
                    int retries = 0;

                    int serverPort = -1;
                    while (retries < 3 && !Utils.ValidatePort(serverPort))
                    {
                        Console.WriteLine("Enter the Port the MP Server should listen at:");
                        int.TryParse(Console.ReadLine(), out serverPort);
                        if(!Utils.ValidatePort(serverPort))
                        {
                            retries++;
                            Console.WriteLine("{0}/3 - Invalid Server Port", retries);
                            if (retries > 3) { Environment.Exit(2); }
                        }
                    }
                    retries = 0;

                    string wireguardAddress = null;
                    while (retries < 3 && !Utils.ValidateIpAddress(wireguardAddress))
                    {
                        Console.WriteLine("Enter the address of Wireguard (xxx.xxx.xxx.xxx):");
                        wireguardAddress = Console.ReadLine();
                        if (!Utils.ValidateIpAddress(wireguardAddress))
                        {
                            retries++;
                            Console.WriteLine("{0}/3 - Invalid Wireguard Address", retries);
                            if (retries > 3) { Environment.Exit(3); }
                        }
                    }
                    retries = 0;

                    
                    int wireguardPort = -1;
                    while(retries < 3 && (!Utils.ValidatePort(wireguardPort) || serverPort == wireguardPort))
                    {
                        Console.WriteLine("Enter the port of Wireguard:");
                        int.TryParse(Console.ReadLine(), out wireguardPort);
                        if (!Utils.ValidatePort(wireguardPort) || serverPort == wireguardPort)
                        {
                            retries++;
                            Console.WriteLine("{0}/3 - Invalid Wireguard Port{1}", retries, serverPort == wireguardPort ? " - Same port as Listen." : "");
                            if (retries > 3) { Environment.Exit(4); }
                        }
                    }

                    MultiPathConnection mp = new MultiPathConnection(wireguardAddress, wireguardPort, serverPort);
                    mp.StartMultiPath();

                    Config.config.mp_servers.Add(mp);
                    Config.SaveConfig();
                }
            }
        }
    }
}
