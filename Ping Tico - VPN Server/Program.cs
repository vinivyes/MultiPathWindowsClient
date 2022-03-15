using PingTicoVPNServer.Classes;
using PingTicoVPNServer.Modules;
using System;
using System.Collections.Generic;
using System.Threading;

namespace PingTicoVPNServer
{
    enum ExitCode : int
    {
        Success = 0,
        NewMPInvalidServerPort = 2,
        NewMPInvalidWGAddress = 3,
        NewMPInvalidWGPort = 4,
        NewMPServerPortInUse = 5,
    }

    class Program
    {

        static void Main(string[] args)
        {
            Config.LoadConfig();
            LoadArguments(args);
            //LoadArguments(new string[] { "--new-mp-server", "2", "--test" }); //Testing....

            //Keep Alive
            while(true) { Thread.Sleep(1000); }
        }

        //Loads all arguments and apply configuration.
        private static void LoadArguments(string[] a)
        {
            for(int i = 0; i < a.Length; i++)
            {
                if (a[i].ToLower() == "--new-mp-server")
                {
                    int retries = 0; //How many times invalid information was entered. Resets every 
                    int add_mp_server_count = 1; //How many servers to add

                    if (a.Length > (i + 1) ? int.TryParse(a[i + 1], out add_mp_server_count) : false)
                    {
                        i++;
                        if(add_mp_server_count < 1) { continue; }
                        Log.ToConsole(LogLevel.INFO, String.Format("Creating {0} MP Servers...", add_mp_server_count));
                    }

                    do
                    {
                        //Get what port the server will listen on
                        int serverPort = -1;
                        while (retries < 3 && (!Utils.ValidatePort(serverPort) || !Config.config.MultiPathPortFree(serverPort))) //Asks for a valid port up to 3 times. Exits application on failure
                        {
                            Console.WriteLine("Enter the Port the MP Server should listen at:");
                            int.TryParse(Console.ReadLine(), out serverPort);
                            if (!Utils.ValidatePort(serverPort))
                            {
                                retries++;
                                Console.WriteLine("{0}/3 - Invalid Server Port", retries);
                                if (retries > 3) { Environment.Exit((int)ExitCode.NewMPInvalidServerPort); }
                            }
                            if (!Config.config.MultiPathPortFree(serverPort))
                            {
                                retries++;
                                Console.WriteLine("{0}/3 - Server Port already in use", retries);
                                if (retries > 3) { Environment.Exit((int)ExitCode.NewMPServerPortInUse); }
                            }
                        }
                        retries = 0;

                        //Gets what address and port Wireguard is running on.
                        string wireguardAddress = null;
                        int wireguardPort = -1;
                        while (retries < 3 && (!Utils.ValidateIpAddress(wireguardAddress) || !Utils.ValidatePort(wireguardPort) || serverPort == wireguardPort)) //Asks for a valid ip address up to 3 times. Exits application on failure
                        {
                            Console.WriteLine("Enter the address and port of Wireguard <xxx.xxx.xxx.xxx>:<Port #>");
                            string input = Console.ReadLine();

                            //Parse input
                            wireguardAddress = input.Split(':')[0];
                            int.TryParse(input.Contains(':') ? input.Split(':')[1] : "-1", out wireguardPort);  //If address has ':' character, get port text and try to parse.

                            //Validate input
                            if (!Utils.ValidateIpAddress(wireguardAddress))
                            {
                                retries++;
                                Console.WriteLine("{0}/3 - Invalid Wireguard Address", retries);
                                if (retries > 3) { Environment.Exit((int)ExitCode.NewMPInvalidWGAddress); }
                            }
                            if (!Utils.ValidatePort(wireguardPort) || serverPort == wireguardPort)
                            {
                                retries++;
                                Console.WriteLine("{0}/3 - Invalid Wireguard Port{1}", retries, serverPort == wireguardPort ? " - Same port as Listen." : "");
                                if (retries > 3) { Environment.Exit((int)ExitCode.NewMPInvalidWGPort); }
                            }
                        }
                        retries = 0;

                        MultiPathConnection mp = new MultiPathConnection(wireguardAddress, wireguardPort, serverPort);
                        mp.StartMultiPath();

                        Config.config.mp_servers.Add(mp);

                        add_mp_server_count--;
                    }
                    while(add_mp_server_count > 0);
                    
                    Config.SaveConfig();
                }
                if (a[i].ToLower() == "--test")
                {
                    Log.ToConsole(LogLevel.DEBUG, "TEST");
                }
            }
        }
    }
}
