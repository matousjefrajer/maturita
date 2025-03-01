using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace matura
{
    internal class server
    {
        static bool StilSearch = true;
        static int Port = 13000;
        public static void Search()
        {
            string PlayerIP;
            string PlayerName;
            string returnData;

            Console.WriteLine("Hledání ostatních hráčů");
            Console.WriteLine("Stiskni \"B\" pro přidání bota, nebo cokoli pro začátek hry");
            Console.WriteLine("pokud někomu nepřišla zpráva při hře, tak stiskni \"S\", nebo \"R\" pro vyhození hráče");

            UdpClient udpClient = new UdpClient(Port); //propojuju se pres ten 13000 https://learn.microsoft.com/cs-cz/dotnet/api/system.net.sockets.udpclient?view=net-8.0
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, Port); //povolí mi číst zprávy od ostatních zařízení (musim mít povolený port 13000 jako příchozí

            udpClient.Client.ReceiveTimeout = 3000; // přidal jsem timeot, protože by se to zasekávalo při naslouchání na portu

            Thread keyThread = new Thread(SendMessageAgain); //někdy to ukončit chatgpt poradil
            keyThread.Start();


            while (StilSearch)
            {
                IsThereInternet(); //ověřím připojení

                try
                {
                    Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint); //naslouchá nejdriv v bitech
                    returnData = Encoding.UTF8.GetString(receiveBytes); //¨přepíšu do slov pro zjednodušení https://learn.microsoft.com/cs-cz/dotnet/framework/network-programming/using-udp-services
                    
                    if (returnData.Contains("MAUMAUPLAYER")) //ověřim, jestli je to hrac
                    {
                        string[] parts = returnData.Split('.');
                        PlayerName = parts[1]; //počet karet (za tečkou)

                        PlayerIP = RemoteIpEndPoint.Address.ToString(); //prepisu tu ip do stringu
                        Console.WriteLine($"Nalezen hráč s IP: {PlayerIP}");

                        string response = "MAUMAUSERVER";
                        byte[] responseData = Encoding.UTF8.GetBytes(response); // ověř si, kdo to opravdu dostane!!!!!
                        udpClient.Send(responseData, responseData.Length, RemoteIpEndPoint);

                        PlayerList.AddPlayer(PlayerIP, PlayerName);

                        if(PlayerList.playerIPList.Count > 6)
                        {
                            Console.WriteLine($"Byl naplněn možný počet hráčů");
                            StilSearch = false;
                        }
                    }
                }
                catch (SocketException e) when (e.SocketErrorCode == SocketError.TimedOut)
                {
                    Console.WriteLine("Stále hledám...");
                }
            }
            Console.WriteLine("Konec hledání");
            udpClient.Close();
        }
        
        public static bool IsThereInternet()
        {
            TcpClient client = new TcpClient(); //https://learn.microsoft.com/cs-cz/dotnet/api/system.net.sockets.tcpclient?view=net-8.0

            try
            {
                client.Connect("8.8.8.8", 53); // 8.8.8.8 je veřejná ip adresa googlu a port 53
                return true;
            }
            catch (SocketException e)
            {
                Console.WriteLine($"Žádné připojení k internetu (error message: {e.SocketErrorCode})");
                return false;
            }
        }

        static void SendMessageAgain()
        {
            while (StilSearch)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey();
                    if (key.Key == ConsoleKey.B)  
                    {
                        if (PlayerList.playerIPList.Count > 6)
                        {
                            Console.WriteLine($"Byl naplněn možný počet hráčů");
                            StilSearch = false;
                        }
                        else
                        {
                            PlayerList.AddBot();
                        }                        
                    }
                    else 
                    {
                        Console.WriteLine("\nKonec hledání");
                        StilSearch = false; //ukonci vsehcno
                    }
                }
                Thread.Sleep(100); // zpoždění, aby se CPU nezatěžovalo (doporucil chatgpt - chci prokonzultovat)
            }
        }
    }
}
