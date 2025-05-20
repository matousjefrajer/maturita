using System.Net.Sockets;
using System.Net;
using System.Text;

namespace matura
{
    internal class Player_Client
    {
        private static bool stillSend = true;
        public static IPEndPoint serverEndPoint = new IPEndPoint(0, 0);
        public static int clientPort = 0;
        public static string nick = "";
              
        static public void Search()
        {
            UdpClient udpClient = new UdpClient();
                      
            IPEndPoint IPEndPoint = new IPEndPoint(IPAddress.Broadcast, GlobalSetting.serverPort);
                        
            udpClient.Client.ReceiveTimeout = 3000;
            
            if (GlobalSetting.serverAndPlayerOnOneDevice == false )
            {
                Console.WriteLine("Hledání serveru...");
            }
            
            while (stillSend)
            {
                try
                {
                    string Message = $"MAUMAUPLAYER.{nick}"; 
                    byte[] MessegeData = Encoding.UTF8.GetBytes(Message);
                    udpClient.Send(MessegeData, MessegeData.Length, IPEndPoint);

                    clientPort = (udpClient.Client.LocalEndPoint as IPEndPoint)?.Port ?? 0;

                    serverEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    byte[] serverResponse = udpClient.Receive(ref serverEndPoint);
                    string responseMessage = Encoding.UTF8.GetString(serverResponse);
                    
                    if (responseMessage == "MAUMAUSERVER") 
                    {
                        if (GlobalSetting.serverAndPlayerOnOneDevice == false) Console.WriteLine($"Našel jsi server. Počkej, až se všichni připojí a spustí se hra.");
                        else Console.WriteLine($"Stiskni \"B\" pro přidání bota, \"V\" pro vyhození hráče, nebo \"K\" pro konec hledání a začátek hry.");

                        stillSend = false;
                    }
                    else if (responseMessage == "TAKENNAME")
                    {
                        Console.WriteLine($"Jméno {nick} je již zabrané, zkus zadat nové:");
                        GlobalSetting.EnterNick();
                    }
                }
                catch (SocketException e) when (e.SocketErrorCode != SocketError.TimedOut)
                {
                    Console.WriteLine($"Chyba {e}");
                }
                catch (Exception) { }
            }
            udpClient.Close();
        }
    }
}
