using Ping_Tico___VPN_Server.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Ping_Tico___VPN_Server
{
    class Program
    {

        private static UdpClient server;

        private static int WireguardPort = 30000;
        private static int ServerPort = 21212;

        private static string WireguardAddress = "127.0.0.1";
        static void Main(string[] args)
        {
            LoadArguments(args);

            server = new UdpClient(ServerPort);

            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            IPEndPoint WireguardIpEndpoint = new IPEndPoint(IPAddress.Parse(WireguardAddress), WireguardPort);

            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
                ProtocolType.Udp);

            sock.Connect(WireguardIpEndpoint);
            Console.WriteLine("Connected to Wireguard on " + WireguardIpEndpoint.Address + ":" + WireguardIpEndpoint.Port);

            Dictionary<IPEndPoint, Route> routes = new Dictionary<IPEndPoint, Route>();

            _ = Task.Run(() => {
                while (true)
                {
                    try
                    {
                        Byte[] receiveBytes = server.Receive(ref RemoteIpEndPoint);

                        sock.Send(receiveBytes);


                        if (!routes.ContainsKey(RemoteIpEndPoint))
                        {
                            Console.WriteLine("Registering " + RemoteIpEndPoint.Address + ":" + RemoteIpEndPoint.Port);

                            Route r = new Route(RemoteIpEndPoint.Address, RemoteIpEndPoint.Port);

                            r.SetActiveStatus(true);

                            routes.Add(RemoteIpEndPoint, r);
                        }
                    }
                    catch { }
                }
            });

            _ = Task.Run(() => {
                while (true)
                {
                    try
                    {
                        byte[] receivedBytes = new byte[4098];
                        int readBytes = sock.Receive(receivedBytes, SocketFlags.None);

                        byte[] cropBytes = new byte[readBytes];
                        for (int b = 0; b < readBytes; b++)
                        {
                            cropBytes[b] = receivedBytes[b];
                        }

                        foreach (KeyValuePair<IPEndPoint, Route> r in routes)
                        {
                            server.Send(cropBytes, cropBytes.Length, r.Key);
                        }
                    }
                    catch { }
                }
            });

            while(true) { Thread.Sleep(1000); }
        }
    
        private static void LoadArguments(string[] a)
        {
            for(int i = 0; i < a.Length; i++)
            {
                if(a[i] == "--WireguardPort")
                {
                    int.TryParse(a[i + 1], out WireguardPort);
                    i++;
                }
                if (a[i] == "--ServerPort")
                {
                    int.TryParse(a[i + 1], out ServerPort);
                    i++;
                }
                if (a[i] == "--WireguardAddress")
                {
                    WireguardAddress = a[i + 1];
                    i++;
                }
            }
        }
    }
}
