using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Net.Mail;

namespace matura_2pc
{
    internal class Client
    {
        public static bool StillSend = true;
        public static string ServerIP = "0";
        public static int Port = 13000;
        public static string Nick = "";

        static public void Search()
        {
            

            UdpClient udpClient = new UdpClient();
            //udpClient.EnableBroadcast = true;

            IPEndPoint broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, Port);

            udpClient.Client.ReceiveTimeout = 3000;

            while (StillSend)
            {
                try
                {
                    Console.WriteLine("Hledání serveru...");

                    string Message = $"MAUMAUPLAYER.{Nick}"; // nápad bylo, jakože bude vysílat "heslo", aby se tam nemohl připojit nikdo jiný, kdo to heslo nemá
                    byte[] MessegeData = Encoding.UTF8.GetBytes(Message);
                    udpClient.Send(MessegeData, MessegeData.Length, broadcastEndPoint);

                    IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Any, Port); //tady je to sus, uz muzes psat na konkretni ip
                    byte[] serverResponse = udpClient.Receive(ref serverEndPoint);
                    string responseMessage = Encoding.UTF8.GetString(serverResponse);
                    //Console.WriteLine($"prislo: {responseMessage}");
                    if (responseMessage == "MAUMAUSERVER") //ověřim, jestli je to ten správný server
                    {
                        ServerIP = serverEndPoint.Address.ToString();

                        //Console.WriteLine($"Server found at IP: {ServerIP}");
                        //Console.WriteLine("you are in");

                        StillSend = false;
                    }
                    
                }
                catch (SocketException e) when (e.SocketErrorCode == SocketError.TimedOut)
                {
                    //Console.WriteLine("konec");
                }
            }
            
            udpClient.Close();
        }
       
    }
    
}
