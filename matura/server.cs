using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace matura
{
    internal class server
    {
        //static List<Player> playerIPList = new List<Player>();  // Seznam IP adres připojených hráčů https://learn.microsoft.com/cs-cz/dotnet/api/system.collections.generic.list-1?view=net-8.0
        static bool StilSearch = true;
        static int Port = 13000;
        public static void Search()
        { 
            string PlayerIP;
            string returnData;


            Console.WriteLine("Searching for other players");
            Console.WriteLine("press eny key to stop searching");

            

            UdpClient udpClient = new UdpClient(Port); //propojuju se pres ten 13000 https://learn.microsoft.com/cs-cz/dotnet/api/system.net.sockets.udpclient?view=net-8.0
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, Port); //povolí mi číst zprávy od ostatních zařízení (musim mít povolený port 13000 jako příchozí

            udpClient.Client.ReceiveTimeout = 3000; // přidal jsem timeot, protože by se to zasekávalo při naslouchání na portu

            Thread keyThread = new Thread(CheckForKeyPress); //někdy to ukončit chatgpt poradil
            keyThread.Start();

            
            while (StilSearch)
            {
                Console.WriteLine("kolo");
                
                IsThereInternet(); //ověřím připojení

                try
                {
                    //IPEndPoint foundPlayer = new IPEndPoint(IPAddress.Any, Port);
                    Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint); //naslouchá nejdriv v bitech
                    returnData = Encoding.ASCII.GetString(receiveBytes); //¨přepíšu do slov pro zjednodušení https://learn.microsoft.com/cs-cz/dotnet/framework/network-programming/using-udp-services
                    //Console.WriteLine(returnData);
                    if (returnData == "MAUMAUPLAYER") //ověřim, jestli je to hrac
                    {
                        PlayerIP = RemoteIpEndPoint.Address.ToString(); //prepisu tu ip do stringu
                        Console.WriteLine($"there is player at {PlayerIP}");

                        
                        string response = "MAUMAUSERVER";
                        byte[] responseData = Encoding.ASCII.GetBytes(response); // ověř si, kdo to opravdu dostane!!!!!
                        udpClient.Send(responseData, responseData.Length, RemoteIpEndPoint);

                        PlayerList.AddPlayer(PlayerIP);




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

                    }
                    
                }
                catch (SocketException e) when (e.SocketErrorCode == SocketError.TimedOut)
                {
                    Console.WriteLine("mas moznost na konec");
                }
                        
                
                //PrintPlayerList();
                
                // smaž pak
                /*
                Console.WriteLine("koho chces videt");
                string vstup = Console.ReadLine();
                int.TryParse(vstup, out int cisloborce);
                string iphracecochcividet = playerIPList[cisloborce - 1]; // -1, protože indexy v seznamu začínají od 0
                Console.WriteLine($"IP adresa hráče číslo {cisloborce} je: {iphracecochcividet}");
                */
                /*if (Console.KeyAvailable)
                {
                    Console.WriteLine("searching done1");
                    Console.ReadKey(intercept: true); // jakákoli klávesa (jen jí zachytim a nezobrazim)
                    Console.WriteLine("searching done2");
                    StilSearch = false; 
                }*/
            }
            Console.WriteLine("fakt to skoncilo");
            udpClient.Close();
        }
        /*
        public static void ServerComunication(int PlayerNumber) 
        {
            Player player = PlayerList.playerIPList[PlayerNumber];
            player.PlayersCards.Add(PackofCards.deck[0]);
        }*/
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
                Console.WriteLine($"no internet connection (error message: {e.SocketErrorCode})");
                return false;
            }
        }
        
        static void CheckForKeyPress()
        {
            while (StilSearch == true)
            {
                if (Console.KeyAvailable)
                {
                    Console.ReadKey(true); //zachycení stisknuté klávesy bez jejího zobrazení
                    Console.WriteLine("neco si zmackl");
                    StilSearch = false; //ukonci vsehcno

                }
                Thread.Sleep(100); // zpoždění, aby se CPU nezatěžovalo (doporucil chatgpt - chci prokonzultovat)
            }
        }
    }
}
