using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections.ObjectModel;
using PingTicoVPN.Classes;

namespace PingTicoVPN.Modules
{
    public static class MultiPath
    {
        private static Thread receiveDataThread; //Thread that will receive data.
        private static bool receivingData = false; //Is currently receiving data.

        private static ViewModel vm;

        private static UdpClient dataReceiver = new UdpClient(31311);                     //What port to receive information on.
        private static IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);    //Where is information being received from.

        //Starts thread that will receive data
        public static void StartReceivingDataFromWireguard(ref ViewModel _vm) {
            //Clears any currently running thread
            if (receiveDataThread != null)
            {
                if (receiveDataThread.IsAlive) {
                    return;
                }
                try
                {
                    receivingData = false;
                    receiveDataThread.Abort();
                }
                catch { }
            }

            vm = _vm;

            //Creates a new Thread
            receiveDataThread = new Thread(
                    new ThreadStart(ReceiveDataFromWireguard)
                );

            //Starts the new Thread
            receiveDataThread.Start();
        }

        private static void ReceiveDataFromWireguard()
        {
            
            receivingData = true;
            while (receivingData)
            {
                try
                {
                    Byte[] receiveBytes = dataReceiver.Receive(ref RemoteIpEndPoint);
                
                    foreach(Route r in vm.RouteList)
                    {
                        if (r.active)
                        {
                            r.ReplicatePacket(receiveBytes);
                        }
                    }
                }
                catch { }
            }
        }

        //Sends data received on Routes back to Wireguard
        public static void SendDataBackToWireguard(byte[] data)
        {
            dataReceiver.Send(data, data.Length, RemoteIpEndPoint);
        }
    }
}
