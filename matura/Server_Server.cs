using System.Net;
using System.Net.Sockets;
using System.Text;

namespace matura
{
    internal class Server_Server
    {
        private static bool stillSearch = true;
        public static UdpClient udpClient = new UdpClient();
        public static UdpClient returnUdpClient = new UdpClient();
        public static bool takenName;
        
        public static void Search()
        {
            if (GlobalSetting.serverAndPlayerOnOneDevice == false)
            {
                Console.WriteLine("Spustil se server.");
                Console.WriteLine("Stiskni \"B\" pro přidání bota, \"V\" pro vyhození hráče, nebo \"K\" pro konec hledání a začátek hry. " +
                    "\nPokud si chtěl zvolit něco jineho, stiskni \"Z\".");
                Console.WriteLine("Hledání ostatních hráčů...");                
            }

            try
            {
                udpClient = new UdpClient(GlobalSetting.serverPort); //https://learn.microsoft.com/cs-cz/dotnet/api/system.net.sockets.udpclient?view=net-8.0
                returnUdpClient = new UdpClient(GlobalSetting.returnPort);

                udpClient.Client.ReceiveTimeout = 3000; 
                returnUdpClient.Client.ReceiveTimeout = 3100;
            }
            catch (SocketException) 
            {
                Console.Clear();
                Console.WriteLine($"Snažíš se spustit 2. server na jednom ID, změň si s hráči ID hry a budete moct hrát.");
                GlobalSetting.RestartGame();
            }
            
            Thread keyThread = new Thread(WhichKeay); 
            keyThread.Start();

            while (stillSearch)
            {
                IsThereInternet();

                LookForPlayer();
            }

            if (GlobalSetting. serverAndPlayerOnOneDevice == false)
            {
                Console.WriteLine("Konec hledání");
            }
        }
        private static void LookForPlayer()
        {
            string PlayerName;
            string returnData;
            try
            {
                IPEndPoint IpEndPoint = new IPEndPoint(IPAddress.Any, 0); 
                Byte[] receiveBytes = udpClient.Receive(ref IpEndPoint);
                returnData = Encoding.UTF8.GetString(receiveBytes); //přepíšu do slov pro zjednodušení https://learn.microsoft.com/cs-cz/dotnet/framework/network-programming/using-udp-services

                if (returnData.Contains("MAUMAUPLAYER")) 
                {
                    string[] parts = returnData.Split('.');
                    PlayerName = parts[1];

                    takenName = false;

                    PlayerList.AddPlayer(IpEndPoint, PlayerName);
                    
                    string response;
                    if (takenName == false)
                    {
                        response = "MAUMAUSERVER";
                    }
                    else
                    {
                        response = "TAKENNAME";
                    }
                    
                    byte[] responseData = Encoding.UTF8.GetBytes(response); 
                    udpClient.Send(responseData, responseData.Length, IpEndPoint);

       
                    if(PlayerList.playerIPList.Count > 6)
                    {
                        if (GlobalSetting.serverAndPlayerOnOneDevice == false)
                        {
                            Console.WriteLine($"Byl naplněn možný počet hráčů");
                        }
                        stillSearch = false;
                        GlobalSetting.endOfServer = true;
                    }
                }
            }
            catch (SocketException e) when (e.SocketErrorCode == SocketError.TimedOut) { }
            catch (SocketException e)
            {
                if (GlobalSetting.serverAndPlayerOnOneDevice == false)
                {
                    Console.WriteLine($"Chyba: {e}");
                }
            }

        }
        private static bool IsThereInternet()
        {
            TcpClient client = new TcpClient(); //https://learn.microsoft.com/cs-cz/dotnet/api/system.net.sockets.tcpclient?view=net-8.0

            try
            {
                client.Connect("8.8.8.8", 53); // 8.8.8.8 je veřejná ip adresa googlu a port 53 
                return true;
            }
            catch (SocketException e)
            {
                if (GlobalSetting.serverAndPlayerOnOneDevice == false)
                {
                    Console.WriteLine($"Žádné připojení k internetu (error message: {e.SocketErrorCode})");
                }
                return false;
            }
        }
        private static void WhichKeay()
        {
            while (stillSearch)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.B)  
                    {
                        if (PlayerList.playerIPList.Count > 6)
                        {
                            if (GlobalSetting.serverAndPlayerOnOneDevice == false)
                            {
                                Console.WriteLine($"Byl naplněn možný počet hráčů");
                            }
                            stillSearch = false;
                            GlobalSetting.endOfServer = true;
                        }
                        else
                        {
                            PlayerList.AddBot();
                        }                        
                    }
                    else if (key.Key == ConsoleKey.K)
                    {
                        Console.WriteLine("Jsi si opravdu jistý? Pokud ano, stiskni Enter.");
                        if (Console.ReadKey(true).Key == ConsoleKey.Enter)
                        {
                            Console.WriteLine("\nKonec hledání");

                            stillSearch = false; //ukonci vsehcno
                            GlobalSetting.endOfServer = true;
                        }
                    }
                    else if (key.Key == ConsoleKey.V)
                    {
                        KickPlayer();
                    }
                    else if (key.Key == ConsoleKey.Z)
                    {
                        Console.WriteLine("Jsi si opravdu jistý? Pokud ano, stiskni Enter.");
                        if (Console.ReadKey(true).Key == ConsoleKey.Enter)
                        {
                           GlobalSetting.RestartGame(); 
                        }
                    }
                    else { Console.WriteLine("Zkus to znova"); }
                }
                Thread.Sleep(100); // zpoždění, aby se CPU nezatěžovalo
            }
        }
        private static void KickPlayer()
        {
            foreach (var p in PlayerList.playerIPList)
            {
                Console.WriteLine($"- {p.playerName}");
            }
            Console.WriteLine("Zadej jméno hráče, kterého chceš vyhodit, nebo \"nikdo\":");
            string playertokick;
            bool done = false;
            do
            {
                playertokick = Console.ReadLine() ?? "";

                if (string.IsNullOrWhiteSpace(playertokick))
                {
                    Console.WriteLine("Musíš něco zadat.");
                }
                else
                {
                    Player? playerToKick = PlayerList.playerIPList.FirstOrDefault(p => p.playerName == playertokick);
                    if (playertokick == "nikdo")
                    {
                        Console.WriteLine("Nikdo nebyl vyhozen.");
                        break;
                    }
                    else if (playerToKick == null)
                    {
                        Console.WriteLine($"Hráč {playertokick} nebyl nalezen.");
                    }                    
                    else
                    {
                        Console.WriteLine("Jsi si opravdu jistý? Pokud ano, stiskni Enter.");
                        if (Console.ReadKey(true).Key == ConsoleKey.Enter)
                        {
                            if (playerToKick.IPEndPoint != null)
                            {
                                string message = "YOUWEREKICKED";
                                byte[] responseData = Encoding.UTF8.GetBytes(message);
                                udpClient.Send(responseData, responseData.Length, playerToKick.IPEndPoint);
                            }
                            PlayerList.playerIPList.Remove(playerToKick);
                            Console.WriteLine($"Hráč {playertokick} byl vyhozen.");

                            string Message = $"byl vyhozen";

                            Server_Game.SendGameInfo(playerToKick, Message);
                            done = true;
                        }
                    }
                }

            } while (done == false);
        }
    }

}
