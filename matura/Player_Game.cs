using System.Net.Sockets;
using System.Net;
using System.Text;

namespace matura
{
    internal class Player_Game
    {
        private static bool endofgame = true;
        private static bool SkipInput;
        private static string receivedMessage = "";
        private static int CardCount = 0;
        private static bool ScoreBoard = false;
        private static readonly object lockObj = new object();
        private static UdpClient MainUdpClient= new UdpClient(Player_Client.ClientPort);
        
        public static void Comunication()
        {
            Thread keyThread = new Thread(Keypress);
            keyThread.Start();

            if (GlobalSetting.SaPOnOneDevice == false) Player_Visuals.ServerPlayer = "Pokud chceš opustit hru, tak stiskni \"O\".";
            
            while (endofgame)
            {
                SkipInput = false;

                try
                {
                    IPEndPoint EndPoint = new IPEndPoint(Player_Client.ServerEndPoint.Address, Player_Client.ClientPort);
                    receivedMessage = ReceiveMessage(MainUdpClient, EndPoint);
                    DecryptionOfTheMessage();

                    if (ScoreBoard == true) break;

                    if (SkipInput == false)
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
            if (GlobalSetting.SaPOnOneDevice == false) GlobalSetting.RestartGame(); //pokud je to na jedom zařízení, tak chci nechat server běžet
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
                    endofgame = false;
                    ScoreBoard = true;
                    break;

                case string msg when msg.Contains("DOHRÁL jsi"):
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{receivedMessage}"); ;
                    Player_Visuals.ServerPlayer = "DOHRÁL JSI - " + Player_Visuals.ServerPlayer;
                    Console.ResetColor();
                    SkipInput = true;
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
                    Player_Visuals.ServerPlayer = "VYHRÁL JSI - " + Player_Visuals.ServerPlayer;
                    SkipInput = true;
                    
                    break;

                case string msg when msg.Contains("informace"):
                    parts = receivedMessage.Split('.');
                    Player_Visuals.UpdateHistory(parts[0]);
                    
                    players = parts[1].Split("\n", StringSplitOptions.RemoveEmptyEntries);
                    Player_Visuals.UpdatePlayers(players);

                    Player_Visuals.UpdateScreen();

                    SkipInput = true;
                    break;

                case string msg when msg.Contains("firstinfo"):
                    parts = receivedMessage.Split('.');

                    players = parts[1].Split("\n", StringSplitOptions.RemoveEmptyEntries);
                    Player_Visuals.UpdatePlayers(players);

                    Cards = parts[0].Split(',');

                    Player_Visuals.Cards = Cards[1];
                    Player_Visuals.LastCard = Cards[0];
                    Player_Visuals.UpdateScreen();

                    SkipInput = true;
                    break;

                case string msg when msg.Contains("onturn"):
                    parts = receivedMessage.Split('.');
                    Player_Visuals.WhoIsOnTurn = parts[0];

                    SkipInput = true;
                    Player_Visuals.UpdateScreen();
                    break;

                case "MAUMAUPLAYER":
                case "TAKENNAME":
                    SkipInput = true; 
                    break;
                case "YOUWEREKICKED":
                    Console.Clear();
                    Console.WriteLine($"Byl jsi vyhozen.");
                    SkipInput = true;
                    break;

                case string msg when msg.Contains("zkus to znova"):
                    Console.WriteLine($"     {receivedMessage}");
                    break;

                case string msg when msg.Contains("jakou barvu chceš"): 
                    Console.WriteLine($"{receivedMessage}");
                    CardCount = 5; //vybírá ze 4 barev, ale necham to jako karty pro zjednodušení kodu
                    Console.WriteLine("     Napiš číslo barvy, kterou chceš");
                    break;
                case string msg when msg.Contains("Otáčí se") || msg.Contains("Rozdaly se") || msg.Contains("DOHRÁL") || msg.Contains("VYHRÁL") || msg.Contains("ZAČALA HRA"):
                    Console.WriteLine($"{receivedMessage}");
                    Player_Visuals.UpdateHistory(receivedMessage);
                    SkipInput = true;
                    break;
                default:
                    parts = receivedMessage.Split('.');
                    Cards = parts[0].Split('|');

                    Player_Visuals.Cards = Cards[1];
                    Player_Visuals.LastCard = Cards[0];

                    CardCount = int.Parse(parts[1]);
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
                if (GlobalSetting.EndOfServer == true)
                {
                    if (Console.KeyAvailable)
                    {
                        ConsoleKeyInfo key = Console.ReadKey();
                        if (key.Key == ConsoleKey.O)
                        {
                            Console.WriteLine("Jsi si opravdu jsit?, pokud ano stiskni Enter");
                            if (Console.ReadKey(true).Key == ConsoleKey.Enter)
                            {
                                LeaveGame();
                            }
                        }
                        else if (GlobalSetting.SaPOnOneDevice == true)
                        {
                            if (key.Key == ConsoleKey.V)
                            {
                                Server_Game.KickPlayer();
                            }
                            else if (key.Key == ConsoleKey.K)
                            {
                                Console.WriteLine("Jsi si opravdu jsit?, pokud ano stiskni Enter");
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

            IPEndPoint IPEndPoint = new IPEndPoint(IPAddress.Broadcast, GlobalSetting.ReturnPort);

            udpClient.Client.ReceiveTimeout = 3000;
            try
            {
                string Message = $"KICKME.{Player_Client.Nick}";

                SendMessage(Message, udpClient, IPEndPoint);

                Player_Client.ClientPort = (udpClient.Client.LocalEndPoint as IPEndPoint)?.Port ?? 0;

                
                Player_Client.ServerEndPoint = new IPEndPoint(IPAddress.Any, 0);
                
                string responseMessage = ReceiveMessage(udpClient, Player_Client.ServerEndPoint);
                                
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

            if (GlobalSetting.SaPOnOneDevice == false) GlobalSetting.RestartGame();
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
                    if (Input?.ToUpper() == "V" && GlobalSetting.SaPOnOneDevice == true)
                    {
                        Server_Game.KickPlayer();
                        wronginput = false;
                    }
                    else if (Input?.ToUpper() == "K" && GlobalSetting.SaPOnOneDevice == true)
                    {
                        Console.WriteLine("Jsi si opravdu jsit?, pokud ano stiskni Enter");
                        if (Console.ReadKey(true).Key == ConsoleKey.Enter)
                        {
                            Server_Game.EndGame();
                        }
                    }
                    else if (Input?.ToUpper() == "O")
                    {
                        Console.WriteLine("Jsi si opravdu jsit?, pokud ano stiskni Enter");
                        if (Console.ReadKey(true).Key == ConsoleKey.Enter)
                        {
                            LeaveGame();
                        }
                    }
                    else if (int.TryParse(Input, out int response))
                    {

                        if (response < CardCount && response > 0 || !receivedMessage.Contains("jakou barvu chceš") && response <= CardCount) //&& response > 0
                        {
                            Thread.Sleep(100); // bez něj se to občas sekne když je někdo rychlej
                            IPEndPoint GameServerEndPoint2 = new IPEndPoint(Player_Client.ServerEndPoint.Address, GlobalSetting.ServerPort);
                            SendMessage(Input, MainUdpClient, GameServerEndPoint2);
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
                                Console.WriteLine($"     Máš pouze {CardCount} karet");
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
