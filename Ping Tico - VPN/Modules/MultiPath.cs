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

        //Function to receive data from Wireguard while 'receivingData' is true. Called as a new Thread
        private static void ReceiveDataFromWireguard()
        {
            
            receivingData = true;
            while (receivingData)
            {
                try
                {
                    Byte[] receiveBytes = dataReceiver.Receive(ref RemoteIpEndPoint); //Receive data...
                
                    foreach(Route r in vm.RouteList)  //For each route...
                    {
                        if (r.active)  //Currently Active...
                        {
                            r.ReplicatePacket(receiveBytes); //Send a copy of the received data.
                        }
                    }
                }
                catch (Exception ex) {
                    Log.HandleError(ex);
                }
            }
        }

        //Sends data received on Routes back to Wireguard
        public static void SendDataBackToWireguard(byte[] data)
        {
            dataReceiver.Send(data, data.Length, RemoteIpEndPoint);
        }
    }
}
