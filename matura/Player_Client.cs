using System.Net.Sockets;
using System.Net;
using System.Text;

namespace matura
{
    internal class Player_Client
    {
        private static bool StillSend = true;
        public static IPEndPoint ServerEndPoint = new IPEndPoint(0, 0);
        public static int ClientPort = 0;
        public static string Nick = "";
              
        static public void Search()
        {
            UdpClient udpClient = new UdpClient();
                      
            IPEndPoint IPEndPoint = new IPEndPoint(IPAddress.Broadcast, GlobalSetting.ServerPort);
                        
            udpClient.Client.ReceiveTimeout = 3000;
            
            if (GlobalSetting.SaPOnOneDevice == false )
            {
                Console.WriteLine("Hledání serveru...");
            }
            
            while (StillSend)
            {
                try
                {
                    string Message = $"MAUMAUPLAYER.{Nick}"; 
                    byte[] MessegeData = Encoding.UTF8.GetBytes(Message);
                    udpClient.Send(MessegeData, MessegeData.Length, IPEndPoint);

                    ClientPort = (udpClient.Client.LocalEndPoint as IPEndPoint)?.Port ?? 0;

                    ServerEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    byte[] serverResponse = udpClient.Receive(ref ServerEndPoint);
                    string responseMessage = Encoding.UTF8.GetString(serverResponse);
                    
                    if (responseMessage == "MAUMAUSERVER") 
                    {
                        if (GlobalSetting.SaPOnOneDevice == false) Console.WriteLine($"Našel jsi server. Počkej, až se všichni připojí a spustí se hra.");
                        else Console.WriteLine($"Stiskni \"B\" pro přidání bota, \"V\" pro vyhození hráče, nebo \"K\" pro konec hledání a začátek hry.");

                        StillSend = false;
                    }
                    else if (responseMessage == "TAKENNAME")
                    {
                        Console.WriteLine($"Jméno {Nick} je již zabrané, zkus zadat nové:");
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
