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
                    Console.WriteLine("Looking for server...");

                    string Message = "MAUMAUPLAYER"; // nápad bylo, jakože bude vysílat "heslo", aby se tam nemohl připojit nikdo jiný, kdo to heslo nemá
                    byte[] MessegeData = Encoding.ASCII.GetBytes(Message);
                    udpClient.Send(MessegeData, MessegeData.Length, broadcastEndPoint);

                    IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Any, Port); //tady je to sus, uz muzes psat na konkretni ip
                    byte[] serverResponse = udpClient.Receive(ref serverEndPoint);
                    string responseMessage = Encoding.ASCII.GetString(serverResponse);
                    if (responseMessage == "MAUMAUSERVER") //ověřim, jestli je to ten správný server
                    {
                        ServerIP = serverEndPoint.Address.ToString();

                        Console.WriteLine($"Server found at IP: {ServerIP}");
                        Console.WriteLine("you are in");

                        StillSend = false;
                    }
                    
                }
                catch (SocketException e) when (e.SocketErrorCode == SocketError.TimedOut)
                {
                    //Console.WriteLine("konec");
                }
            }
            

           
            
                //try
                //{
                    
                    //PlayerList.AddPlayer(PlayerIP);




                    /*
                    if (!playerIPList.Contains(PlayerIP))
                    {
                        playerIPList.Add(PlayerIP);  // přidání do seznamu
                        int PlayerIndex = playerIPList.Count;  // číslo hráče
                        Console.WriteLine($"Hráč {PlayerIndex} připojen s IP: {PlayerIP}");
                    }
                    else
                    {
                        int playerNumber = playerIPList.IndexOf(PlayerIP) + 1;
                        Console.WriteLine($"Hráč {playerNumber} už je připojen.");
                    }*/


                    /*
                    IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse("10.0.2.9"), 0);
                    byte[] responseData = udpClient.Receive(ref remoteEndPoint);
                    string responseMessage = Encoding.ASCII.GetString(responseData);
                    Console.WriteLine($"Response from server: {responseMessage}");
                    */
                    /*   IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 13000);
                   Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint); //naslouchá nejdriv v bitech
                   string returnData = Encoding.ASCII.GetString(receiveBytes);
                   Console.WriteLine(returnData);*/
                //}
               // catch { Console.WriteLine("prko"); }
            udpClient.Close();
        }
        /*
        static public void Comunication() 
        {
            //Console.WriteLine("zmackni neco");
            //Console.ReadKey();

            int CardCount = 0;
            while (true)
            {
                //UdpClient udpServer = null;
                
                try
                {
                    UdpClient udpServer = new UdpClient(Port);
                    Console.WriteLine("Čekám na zprávu...");

                    IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(ServerIP), Port);
                    
                    byte[] receivedData = udpServer.Receive(ref serverEndPoint);
                    string receivedMessage = Encoding.UTF8.GetString(receivedData);

                    

                    if (receivedMessage != "jsi blbej, zkus to znova") //pokud je to ta normalni zprava co jde delit tak deli, jinak mi ten pocet zustava
                    {
                        string[] parts = receivedMessage.Split('.');

                        string mainMessage = parts[0]; //počtu karet tam neni
                        CardCount = int.Parse(parts[1]); //počet karet (za tečkou)
                        Console.WriteLine($"Přijatá zpráva od {serverEndPoint.Address}: \n{mainMessage}");
                        Console.WriteLine($"počet karet {CardCount} ");
                    }
                    else 
                    {
                        Console.WriteLine($"blbá zpráva od {serverEndPoint.Address}: \n{receivedMessage}");
                        Console.WriteLine($"počet karet {CardCount} ");
                    }
                    
                    

                    Console.WriteLine("write number of the card you wanna play or 0 to draw");
                    //Console.WriteLine($"počet karet {CardCount} ");
                    //int CardCount = receivedMessage.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).Length - 2; //poradil chat
                    
                    //Console.WriteLine($"parag {paragraphCount} ");
                    
                    bool wronginput = true;
                    do {
                       
                        if (int.TryParse(Console.ReadLine(), out int response))
                        {
                            if (response <= CardCount ) //&& response > 0
                            {
                                byte[] sendresponse = Encoding.UTF8.GetBytes(response.ToString());
                                Thread.Sleep(500); // bez něj se to občas sekne když je někdo rychlej
                                udpServer.Send(sendresponse, sendresponse.Length, serverEndPoint);
                                Console.WriteLine("you tried to play");
                                
                                udpServer.Close();
                                wronginput = false;
                            }
                            else
                            {
                                Console.WriteLine($"you have only {CardCount} Cards, or you cant write 0 ");
                                //wronginput = false;
                                
                            }
                        }
                        else
                        {
                            Console.WriteLine("wrong number,try again");
                        }
                    } while(wronginput);



                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Chyba: {ex.Message}");
                    Console.WriteLine("poukud je toto spuštěno na stejném počítači jako server, tak jen chvíli počkej");

                }
                
            }
            
           
        }*/
    }
    
}
