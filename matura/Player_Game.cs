using System.Net.Sockets;
using System.Net;
using System.Text;

namespace matura
{
    internal class Player_Game
    {
        private static bool endOfGame = true;
        private static bool skipInput;
        private static string receivedMessage = "";
        private static int cardCount = 0;
        private static bool scoreBoard = false;
        private static readonly object lockObj = new object();
        private static UdpClient mainUdpClient= new UdpClient(Player_Client.clientPort);
        
        public static void Comunication()
        {
            Thread keyThread = new Thread(Keypress);
            keyThread.Start();

            if (GlobalSetting.serverAndPlayerOnOneDevice == false) Player_Visuals.serverPlayer = "Pokud chceš opustit hru, tak stiskni \"O\".";
            
            while (endOfGame)
            {
                skipInput = false;

                try
                {
                    IPEndPoint EndPoint = new IPEndPoint(Player_Client.serverEndPoint.Address, Player_Client.clientPort);
                    receivedMessage = ReceiveMessage(mainUdpClient, EndPoint);
                    DecryptionOfTheMessage();

                    if (scoreBoard == true) //tady jsi měl chybu - chyběl ten restart
                    { 
                        GlobalSetting.RestartGame(); 
                        break;
                    }

                    if (skipInput == false)
                    {
                        PlayerInput();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Bohužel se něco stalo se serverem a hra jž nepůjde dohrát." +
                        $"\n Chybová zpráva: {ex.Message}");
                }
            }
            if (GlobalSetting.serverAndPlayerOnOneDevice == false) GlobalSetting.RestartGame(); //pokud je to na jedom zařízení, tak chci nechat server běžet
        }
        private static void DecryptionOfTheMessage()
        {
            string[] parts;
            string[] players;
            string[] Cards;

            switch (receivedMessage) 
            {
                case string msg when msg.Contains("Scoreboard"):
                    Console.Clear();
                    Console.WriteLine($"{receivedMessage}");
                    endOfGame = false;
                    scoreBoard = true;
                    break;

                case string msg when msg.Contains("DOHRÁL jsi"):
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{receivedMessage}"); ;
                    Player_Visuals.serverPlayer = "DOHRÁL JSI - " + Player_Visuals.serverPlayer;
                    Console.ResetColor();
                    skipInput = true;
                    break;

                case string msg when msg.Contains("VYHRÁL jsi"):
                    Console.ForegroundColor = ConsoleColor.Red; 
                    Console.WriteLine($"" +
                        $"\n                   _                      _       _         _ " +
                        $"\n                  | |                //  | |     (_)       (_)" +
                        $"\n  __   __  _   _  | |__    _ __    __ _  | |      _   ___   _ " +
                        $"\n  \\ \\ / / | | | | | '_ \\  | '__|  / _` | | |     | | / __| | |" +
                        $"\n   \\ V /  | |_| | | | | | | |    | (_| | | |     | | \\__ \\ | |" +
                        $"\n    \\_/    \\__, | |_| |_| |_|     \\__,_| |_|     | | |___/ |_|" +
                        $"\n            __/ |                               _/ |          " +
                        $"\n           |___/                               |__/           "); //https://patorjk.com/software/taag/?utm_source=chatgpt.com#p=testall&h=0&v=0&f=Graffiti&t=you%20won
                    Console.ResetColor();
                    Thread.Sleep(3000);
                    Player_Visuals.serverPlayer = "VYHRÁL JSI - " + Player_Visuals.serverPlayer;
                    skipInput = true;
                    
                    break;

                case string msg when msg.Contains("informace"):
                    parts = receivedMessage.Split('.');
                    Player_Visuals.UpdateHistory(parts[0]);
                    
                    players = parts[1].Split("\n", StringSplitOptions.RemoveEmptyEntries);
                    Player_Visuals.UpdatePlayers(players);

                    Player_Visuals.UpdateScreen();

                    skipInput = true;
                    break;

                case string msg when msg.Contains("firstinfo"):
                    parts = receivedMessage.Split('.');

                    players = parts[1].Split("\n", StringSplitOptions.RemoveEmptyEntries);
                    Player_Visuals.UpdatePlayers(players);

                    Cards = parts[0].Split(',');

                    Player_Visuals.cards = Cards[1];
                    Player_Visuals.lastCard = Cards[0];
                    Player_Visuals.UpdateScreen();

                    skipInput = true;
                    break;

                case string msg when msg.Contains("onturn"):
                    parts = receivedMessage.Split('.');
                    Player_Visuals.whoIsOnTurn = parts[0];

                    skipInput = true;
                    Player_Visuals.UpdateScreen();
                    break;

                case "MAUMAUPLAYER":
                case "TAKENNAME":
                    skipInput = true; 
                    break;
                case "YOUWEREKICKED":
                    Console.Clear();
                    Console.WriteLine($"Byl jsi vyhozen.");
                    skipInput = true;
                    break;

                case string msg when msg.Contains("zkus to znova"):
                    Console.WriteLine($"     {receivedMessage}");
                    break;

                case string msg when msg.Contains("jakou barvu chceš"): 
                    Console.WriteLine($"{receivedMessage}");
                    cardCount = 5; //vybírá ze 4 barev, ale necham to jako karty pro zjednodušení kodu
                    Console.WriteLine("     Napiš číslo barvy, kterou chceš");
                    break;
                case string msg when msg.Contains("Otáčí se") || msg.Contains("Rozdaly se") || msg.Contains("DOHRÁL") || msg.Contains("VYHRÁL") || msg.Contains("ZAČALA HRA"):
                    Console.WriteLine($"{receivedMessage}");
                    Player_Visuals.UpdateHistory(receivedMessage);
                    skipInput = true;
                    break;
                default:
                    parts = receivedMessage.Split('.');
                    Cards = parts[0].Split('|');

                    Player_Visuals.cards = Cards[1];
                    Player_Visuals.lastCard = Cards[0];

                    cardCount = int.Parse(parts[1]);
                    Player_Visuals.UpdateScreen();

                    Console.WriteLine($"");

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("     Napiš číslo karty, kterou chceš zahrát, nebo 0 pro líznutí si");
                    Console.ResetColor();
                    break;
            }
        }
        private static void Keypress()
        {
            while (true)
            {
                if (GlobalSetting.endOfServer == true)
                {
                    if (Console.KeyAvailable)
                    {
                        ConsoleKeyInfo key = Console.ReadKey();
                        if (key.Key == ConsoleKey.O)
                        {
                            Console.WriteLine("Jsi si opravdu jistý? Pokud ano, stiskni Enter.");
                            if (Console.ReadKey(true).Key == ConsoleKey.Enter)
                            {
                                LeaveGame();
                            }
                        }
                        else if (GlobalSetting.serverAndPlayerOnOneDevice == true)
                        {
                            if (key.Key == ConsoleKey.V)
                            {
                                Server_Game.KickPlayer();
                            }
                            else if (key.Key == ConsoleKey.K)
                            {
                                Console.WriteLine("Jsi si opravdu jistý? Pokud ano, stiskni Enter.");
                                if (Console.ReadKey(true).Key == ConsoleKey.Enter)
                                {
                                    Server_Game.EndGame();
                                }
                            }
                        }
                    }
                    Thread.Sleep(500);                                       
                }
            }              
        }
        private static void LeaveGame()
        {
            UdpClient udpClient = new UdpClient();

            IPEndPoint IPEndPoint = new IPEndPoint(IPAddress.Broadcast, GlobalSetting.returnPort);

            udpClient.Client.ReceiveTimeout = 3000;
            try
            {
                string Message = $"KICKME.{Player_Client.nick}";

                SendMessage(Message, udpClient, IPEndPoint);

                Player_Client.clientPort = (udpClient.Client.LocalEndPoint as IPEndPoint)?.Port ?? 0;

                
                Player_Client.serverEndPoint = new IPEndPoint(IPAddress.Any, 0);
                
                string responseMessage = ReceiveMessage(udpClient, Player_Client.serverEndPoint);
                                
                if (responseMessage == "YOUWEREKICKED") 
                {
                    Console.Clear();
                    Console.WriteLine($"Opustil jsi hru.");
                }
            }
            catch (SocketException e) when (e.SocketErrorCode != SocketError.TimedOut)
            {
                Console.WriteLine($"Chyba: {e}");
            }
            
            udpClient.Close();

            if (GlobalSetting.serverAndPlayerOnOneDevice == false) GlobalSetting.RestartGame();
        }
        private static void SendMessage(string response, UdpClient udp, IPEndPoint endPoint)
        {
            byte[] sendresponse = Encoding.UTF8.GetBytes(response.ToString());
            udp.Send(sendresponse, sendresponse.Length, endPoint);
        }
        private static string ReceiveMessage(UdpClient udp, IPEndPoint endPoint)
        {            
            byte[] receivedData = udp.Receive(ref endPoint);
            return Encoding.UTF8.GetString(receivedData);
        }
        private static void PlayerInput()
        {
            bool wronginput = true;
            do
            {   lock (lockObj)
                {
                    string? Input = Console.ReadLine();
                    if (Input?.ToUpper() == "V" && GlobalSetting.serverAndPlayerOnOneDevice == true)
                    {
                        Server_Game.KickPlayer();
                        wronginput = true; //tady jsi měl chybu
                    }
                    else if (Input?.ToUpper() == "K" && GlobalSetting.serverAndPlayerOnOneDevice == true)
                    {
                        Console.WriteLine("Jsi si opravdu jistý? Pokud ano, stiskni Enter.");
                        if (Console.ReadKey(true).Key == ConsoleKey.Enter)
                        {
                            Server_Game.EndGame();
                        }
                    }
                    else if (Input?.ToUpper() == "O")
                    {
                        Console.WriteLine("Jsi si opravdu jistý? Pokud ano, stiskni Enter.");
                        if (Console.ReadKey(true).Key == ConsoleKey.Enter)
                        {
                            LeaveGame();
                        }
                    }
                    else if (int.TryParse(Input, out int response))
                    {

                        if (response < cardCount && response > 0 || !receivedMessage.Contains("jakou barvu chceš") && response <= cardCount) //&& response > 0
                        {
                            Thread.Sleep(100); // bez něj se to občas sekne když je někdo rychlej
                            IPEndPoint GameServerEndPoint2 = new IPEndPoint(Player_Client.serverEndPoint.Address, GlobalSetting.serverPort);
                            SendMessage(Input, mainUdpClient, GameServerEndPoint2);
                            wronginput = false;
                        }
                        else
                        {
                            if (receivedMessage.Contains("jakou barvu chceš"))
                            {
                                Console.WriteLine($"     Máš poue 4 možnosti a nesmíš 0 ");
                            }
                            else
                            {
                                Console.WriteLine($"     Máš pouze {cardCount} karet");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("     Zkus to znova");
                    }
                }                 
            } while (wronginput);
        }
    }    
}
