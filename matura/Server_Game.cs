using System.Net;
using System.Net.Sockets;
using System.Text;

namespace matura
{
    internal class Server_Game
    {
        public static int SevenCount = 0;
        public static bool AceFactor = false;
        public static bool QueenFactor = false;

        public static string Color = "";
        private static string AceMessage = "";
        private static string SevenMessage = "";

        public static bool MoreThenOnePlayer = true;
        public static string GameInfo = "";
        private static Card playedCard = null!;
        public static bool ChangedBotColor = false;
        private static string ColorInfo = "";
        private static Player? PlayerOnTurn;
        private static string MessageToWinner = "VYHRÁL jsi";
        private static string messagetoall = "VYHRÁL";
        private static string Message = "";

        private static List<Player> WinnerList = new List<Player>();
        private static List<Player> ToKickList = new List<Player>();
        
        public static void game()
        {   
            if (GlobalSetting.SaPOnOneDevice == false)
            {
                Console.WriteLine("\nNové kolo\n");
            }
            
            Thread ReconnectThread = new Thread(ReturnToGame);
            ReconnectThread.Start();

            if (GlobalSetting.SaPOnOneDevice == false)
            {
                Thread keyThread = new Thread(Keypress); 
                keyThread.Start();
            }
            
            foreach (var player in PlayerList.playerIPList)
            {
                CallingPlayerOnTurn(player);
            }
            RemoveWinners();
        }
        private static void CallingPlayerOnTurn(Player player)
        {
             PlayerOnTurn = player;
            
            if (ToKickList.Contains(player)) //pokud byl odebrán již v průběhu kola, tak se přeskočí
            {
                return;
            }

            Card lastcard = PackofCards.discardpile.Last();            

            string WhoIsOnTurn = $"{player.PlayerName}.onturn";
            SendToAll(WhoIsOnTurn);

            int cardCount = player.PlayersCards.Count;

            string handCards = string.Join("\n     ", player.PlayersCards.Select((card, index) => $"{index + 1}: {card}")); //chatGPT mi řekl, jak použít string.join
            ColorInfo = "";

            string lastCardString = lastcard.ToString();
            if (!string.IsNullOrEmpty(Color))
            {
                ColorInfo = $" -> barva je: {Color}";
            }

            Message = $"Na vrchu balíčku: {lastCardString}{ColorInfo}{AceMessage}{SevenMessage}|" +
                $"\n     Karty v ruce:" +
                $"\n     {handCards}" +
                $".{cardCount}";

            FirstInfo(lastcard, ColorInfo); //aby se hráčům vyplňila tabulka
            SendMessage(Message, player);

            GettingResponse(player, lastcard);
        }
        private static void GettingResponse(Player player,Card lastcard)
        { 
            int response;
            
            do
            {
                if (GlobalSetting.SaPOnOneDevice == false)
                {
                    Console.WriteLine("Čekám na odpověď...");
                }
                
                if (player.IPEndPoint == null)
                {
                    Bot bot = (Bot)player;
                    response = bot.BotPlayCard(bot, lastcard);
                }
                else response = ReceiveMessage(player);
                
                if (response == -1) return; //přeskočení hráče, co byl vohozen/odešel
                
                if (response == 0)
                {
                    DrawCardEffect(player);
                    return;
                }

                bool NewTry = CheckGameLogick(lastcard, player, response); 

                if (QueenFactor == true && player.IPEndPoint != null)
                {
                    QueenEffect(player);
                }
                else QueenFactor = false;
                
                if (NewTry == true)
                {
                    string Message2 = "Toto nemůžeš, zkus to znova";
                    SendMessage(Message2, player);
                }
                else 
                {
                    SendGameInfo(player, GameInfo);
                    Win(player);
                    break;
                }

            } while (true);

            if (response == -1) 
            { 
                return;
            }
        }
        private static void DrawCardEffect(Player player)
        {
            int NumberOfCards = 1;

            if (SevenCount > 0)
            {
                NumberOfCards = SevenCount * 2;
                SevenMessage = " -> na tebe neplatí";
                SevenCount = 0;
            }

            if (AceFactor == true)
            {
                GameInfo = "stál";
                AceMessage = " -> někdo jiný už stál";
                AceFactor = false;
            }
            else
            {
                GameInfo = $"si {NumberOfCards} krát lízl ";
                PackofCards.DrawCard(player, NumberOfCards);
            }           

            SendGameInfo(player, GameInfo);
        }
        private static void QueenEffect(Player player)
        {
            string Message3 = "     jakou barvu chceš:" +
                "\n     1) srdce" +
                "\n     2) kule" +
                "\n     3) listy" +
                "\n     4) žaludy";

            SendMessage(Message3, player);

            int responseColor;
            
             responseColor = ReceiveMessage(player);
            
            if (responseColor == -1) // pokud by byl hráč vyhozen v tuto chvíli, tak se za něj vybere náhodná barva, aby hra mohla pokračovat
            {
                Random rnd = new Random();
                responseColor = rnd.Next(1, 5);
            }
            Color = GetCardColor(responseColor);
            
            GameInfo = $"zahrál svrška a změnil na {Color}";

            QueenFactor = false;
        }
        private static bool CheckGameLogick(Card CardonDeck, Player player, int CardonHandIndex)
        {
            CardonHandIndex--;
            playedCard = player.PlayersCards[CardonHandIndex];
            GameInfo = $"zahrál {playedCard}"; 

            //na vrchu 7 

            if (CardonDeck.CardValue == "7" && SevenCount > 0)  //první sedma neplatí
            {
                AceMessage = "";
                if (playedCard.CardValue == "7")
                {
                    SevenCount++;
                    SevenMessage = $" -> musíš zahrát 7, nebo si líznout {2 * SevenCount}";
                    PackofCards.PlayCard(CardonHandIndex, player);

                    return false;
                }
                else
                {
                    return true;
                }
            }

            // na vrchu eso

            else if (CardonDeck.CardValue == "Eso" && AceFactor == true) //první eso neplatí
            {
                SevenMessage = "";
                if (playedCard.CardValue == "Eso")
                {
                    PackofCards.PlayCard(CardonHandIndex, player);

                    return false;
                }
                else
                {
                    return true;
                }
            }

            //na vrchu cokoli

            else
            {
                AceMessage = ""; 
                SevenMessage = "";

                if (!string.IsNullOrEmpty(Color) && ChangedBotColor == false)  //když se hraje na svrška, tak se nesmí jeho barva, ale jenom ta zvolenou
                {
                    if (playedCard.CardColor == Color || playedCard.CardValue == "svršek") //na svrška se smí dát svršek
                    {
                        PackofCards.PlayCard(CardonHandIndex, player);
                        PlayedCardProperty();
                        Color = "";

                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else if (ChangedBotColor == true) //pokud je na tahu bot, tak si uz vybral barvu a tady se to přeskočí
                {
                    ChangedBotColor = false;

                    PackofCards.PlayCard(CardonHandIndex, player);
                    PlayedCardProperty();
                    return false;
                }
                else //pokud nebyl svršek
                {
                    if (playedCard.CardColor == CardonDeck.CardColor || playedCard.CardValue == CardonDeck.CardValue || playedCard.CardValue == "svršek")
                    {
                        PackofCards.PlayCard(CardonHandIndex, player);
                        PlayedCardProperty();

                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
        }
        private static void PlayedCardProperty()
        {
            if (playedCard.CardValue == "7") //sedmička při rozdání se nepočítá
            {
                SevenCount++;
                SevenMessage = $" -> musíš zahrát 7, nebo si líznout {2 * SevenCount}";
            }
            else { SevenCount = 0; }
            if (playedCard.CardValue == "Eso") //eso při rozdání se nepočítá
            {
                AceFactor = true;
                AceMessage = " -> musíš stát (stiskni 0), nebo zahrát eso";
            }
            else { AceFactor = false; } 
            if (playedCard.CardValue == "svršek") //svršek při rozdání se nepočítá
            {
                QueenFactor = true;
            }
           
            if (GlobalSetting.SaPOnOneDevice == false)
            {
                Console.WriteLine($"je změněno na {Color}, svršek je {playedCard.CardColor}");
            }
        }
        private static string GetCardColor(int number)
        {
            switch (number)
            {
                case 1: return "Srdce";
                case 2: return "Kule";
                case 3: return "Listy";
                case 4: return "Žaludy";
                default: return "neznámá barva"; // nemělo by nastat
            }
        }
        private static void Win(Player player)
        {
            if (player.PlayersCards.Count == 0)
            {
                if (GlobalSetting.SaPOnOneDevice == false)
                {
                    Console.WriteLine($"{player.PlayerName} Dohrál");
                }

                WinnerList.Add(player);

                SendMessage(MessageToWinner, player);
                MessageToWinner = "DOHRÁL jsi";

                string messagetosend = $"{player.PlayerName} {messagetoall}";
                SendToAll(messagetosend);
                messagetoall = "DOHRÁL";
            }
        }
        private static void SendMessage(string Message, Player player)
        {
            try
            {
                if (player.IPEndPoint == null) //botovi nechci posílat, protože by to vyhazovalo chybu
                {
                    return;
                }
                Thread.Sleep(200); //kvuli sekani

                byte[] sendData = Encoding.UTF8.GetBytes(Message);
                Server_Server.udpClient.Send(sendData, sendData.Length, player.IPEndPoint);
                if (GlobalSetting.SaPOnOneDevice == false)
                {
                    Console.WriteLine($"Odesílám: {Message}");
                }
            }
            catch (Exception ex)
            {
                if (GlobalSetting.SaPOnOneDevice == false)
                {
                    Console.WriteLine($"Chyba: {ex.Message}");
                }
            }
        }
        private static int ReceiveMessage(Player player)
        {
            int response = -1;
            bool stillreceive = true;
            while (stillreceive)
            {
                try
                {
                    if (player.IPEndPoint == null || ToKickList.Contains(player))
                    {
                        return -1;
                    }

                    IPEndPoint responseEndPoint = new IPEndPoint(player.IPEndPoint.Address, GlobalSetting.ServerPort);

                    byte[] receivedData = Server_Server.udpClient.Receive(ref responseEndPoint);
                    response = int.Parse(Encoding.UTF8.GetString(receivedData));
                   
                    stillreceive = false;
                }
                catch (SocketException e) when (e.SocketErrorCode == SocketError.TimedOut)
                {

                }
                catch (Exception ex)
                {
                    if (GlobalSetting.SaPOnOneDevice == false)
                    {
                        Console.WriteLine($"Chyba z receive: {ex.Message}");
                    }
                }                
            }
            return response;
        }
        public static void SendGameInfo(Player PlayerOnTurn, string GameInfo)
        {
            string OthersCards = ""; 
            foreach (var player in PlayerList.playerIPList)
            {
                int cardCount = player.PlayersCards.Count;
                OthersCards += $"\n{player.PlayerName},{cardCount}";
            }
            OthersCards += "\n";

            string GameInfoToSend = $"{PlayerOnTurn.PlayerName} -> {GameInfo}{ColorInfo}.{OthersCards}.informace";
            SendToAll(GameInfoToSend);
        }
        public static void SendToAll(string MessageToAll)
        {
            foreach (var player in PlayerList.playerIPList)
            {
                SendMessage(MessageToAll, player);
            }
            foreach (var player in WinnerList)
            {
                SendMessage(MessageToAll, player);
            }
        }
        private static void RemoveWinners() //or kicked players
        {
            foreach (var player in PlayerList.playerIPList.ToList())
            {
                if (WinnerList.Contains(player) || ToKickList.Contains(player))
                {
                    PlayerList.playerIPList.Remove(player);
                }
            }
            if (PlayerList.playerIPList.Count < 2)
            {
                if (PlayerList.playerIPList.Count == 1)
                {
                    WinnerList.Add(PlayerList.playerIPList[0]);
                }

                SendScoreboard();

                MoreThenOnePlayer = false;
                if (GlobalSetting.SaPOnOneDevice == false) GlobalSetting.RestartGame();
            }
        }
        private static void SendScoreboard()
        {
            int number = 1;
            string MessageToAll = "Scoreboard:";
                        
            var sortedWinners = WinnerList.OrderBy(p => p.PlayersCards.Count).ToList(); //seřadí se podle počtu karet, ti co vyhráli, mají nula a napíšou se tak, jak tam jsou

            for (int i = 0; i < sortedWinners.Count; i++)
            {
                if (i > 0 && sortedWinners[i].PlayersCards.Count != sortedWinners[i - 1].PlayersCards.Count && sortedWinners[i].PlayersCards.Count != 0 || i > 0 && sortedWinners[i].PlayersCards.Count == 0) //hráči s jiným počtem karet jdou na další pozici
                {
                    number++ ; 
                }

                Console.WriteLine($"{number}. {sortedWinners[i].PlayerName}");
                
                MessageToAll += $"\n{number}. {sortedWinners[i].PlayerName}";
            }

            foreach (var PlayerWinner in WinnerList)
            {
                SendMessage(MessageToAll, PlayerWinner);
            }
        }
        private static void FirstInfo(Card lastcard, string ColorInfo)
        {
            string OthersCards = "";
            foreach (var player in PlayerList.playerIPList)
            {
                int cardCount = player.PlayersCards.Count;
                OthersCards += $"\n{player.PlayerName},{cardCount}"; 
            }
            foreach (var player in PlayerList.playerIPList)
            {
                string handCards = string.Join("\n     ", player.PlayersCards.Select((card, index) => $"{index + 1}: {card}")); 

                string message = $"Na vrchu balíčku: {lastcard}{ColorInfo}," +
                    $"\n     Karty v ruce:" +
                    $"\n     {handCards}.{OthersCards}.firstinfo";
                SendMessage(message, player);
            }
        }
        private static void ReturnToGame() //or quit
        {
            while(true)
            {
                try
                {
                    IPEndPoint IpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    Byte[] receiveBytes = Server_Server.ReturnudpClient.Receive(ref IpEndPoint); 

                    string returnData = Encoding.UTF8.GetString(receiveBytes); 

                    string[] parts = returnData.Split('.');
                    string PlayerName = parts[1];
                    
                    if (returnData.Contains("LOOKINGFORSERVER") && PlayerList.playerIPList.Any(player => player.PlayerName == PlayerName && player.IPEndPoint != null && player.IPEndPoint.Address.Equals(IpEndPoint.Address))) //je tam ta kontrola stejné ip, aby se nikdo nepřihlásil za někoho jiného
                    {
                        if (GlobalSetting.SaPOnOneDevice == false)
                        {
                            Console.WriteLine($"Znovu nalezen hráč: {IpEndPoint}");
                        }
                        var player = PlayerList.playerIPList.FirstOrDefault(p => p.PlayerName == PlayerName && p.IPEndPoint?.Address.Equals(IpEndPoint.Address) == true); //to rovna se true je: pokud IPEndPoint neni null, tak to jde dál
                        if (player != null && player.IPEndPoint != null)  
                        {
                            player.IPEndPoint.Port = IpEndPoint.Port;
                            Console.WriteLine($"Port hráče {PlayerName} byl změněn na {IpEndPoint.Port}");

                            string response = "MAUMAUSERVER";
                            SendMessage(response, player);
                        }
                        if (PlayerOnTurn == player && player != null)
                        {
                            Thread.Sleep(200);
                            SendMessage(Message, player);
                        }
                    }
                    else if (returnData.Contains("KICKME"))
                    {
                        var playertokick = PlayerList.playerIPList.FirstOrDefault(p => p.PlayerName == PlayerName);
                        if (playertokick != null)
                        {
                            if (playertokick.PlayersCards.Count > 0)
                            {
                                PackofCards.deck.AddRange(playertokick.PlayersCards); // vrátí se jeho karty
                            }

                            ToKickList.Add(playertokick); //mažou se až na konci kola

                            string response = "YOUWEREKICKED";
                            byte[] responseData = Encoding.UTF8.GetBytes(response);
                            Server_Server.ReturnudpClient.Send(responseData, responseData.Length, IpEndPoint);
                            Console.WriteLine($"{playertokick.PlayerName} opustil hru");

                            string info = "Opustil hru";
                            SendGameInfo(playertokick, info);
                        }
                    }
                    else
                    {
                        string response = "WRONGNAME";
                        byte[] responseData = Encoding.UTF8.GetBytes(response);
                        Server_Server.ReturnudpClient.Send(responseData, responseData.Length, IpEndPoint);
                    }
                }
                catch (SocketException e) when (e.SocketErrorCode == SocketError.TimedOut)
                {

                }
                catch (Exception ex)
                {
                    if (GlobalSetting.SaPOnOneDevice == false)
                    {
                        Console.WriteLine($"Chyba z vlákna: {ex.Message}");
                    }
                }
            }            
        }
        public static void KickPlayer()
        {
            foreach (var p in PlayerList.playerIPList)
            {
                Console.WriteLine($"- {p.PlayerName}");
            }
            
            Console.WriteLine("Zadej jméno hráče, kterého chceš vyhodit, nebo \"nikdo\":");
            string playertokick;
            bool done = false;
            do
            {
                playertokick = Console.ReadLine() ?? "";

                if (playertokick == "nikdo")
                {
                    Console.WriteLine("Nikdo nebyl vyhozen.");
                    break;
                }
                else if (string.IsNullOrWhiteSpace(playertokick))
                {
                    Console.WriteLine("Musíš něco zadat.");
                }
                
                else
                {
                    Player? playerToKick = PlayerList.playerIPList.FirstOrDefault(p => p.PlayerName == playertokick);

                    if (playerToKick == null)
                    {
                        Console.WriteLine($"Hráč {playertokick} nebyl nalezen.");
                    }
                    else
                    {
                        Console.WriteLine("Jsi si opravdu jsit?, pokud ano stiskni Enter");
                        if (Console.ReadKey(true).Key == ConsoleKey.Enter)
                        {
                            if (playerToKick.PlayersCards.Count > 0)
                            {
                                PackofCards.deck.AddRange(playerToKick.PlayersCards); // vrátí se jeho karty
                            }
                                                        
                            ToKickList.Add(playerToKick); //mažou se až na konci kola

                            string info = "Byl vyhozen ze hry";
                            SendGameInfo(playerToKick, info);
                            string message = "YOUWEREKICKED";
                            SendMessage(message, playerToKick);
                            done = true;
                        }
                    }
                }

            } while (done == false);
            Console.WriteLine($"potom");
        }
        private static void Keypress()
        {
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey();
                    if (key.Key == ConsoleKey.V)
                    {
                        KickPlayer();
                    }
                    if (key.Key == ConsoleKey.K)
                    {
                        Console.WriteLine("Jsi si opravdu jsit?, pokud ano stiskni Enter");
                        if (Console.ReadKey(true).Key == ConsoleKey.Enter)
                        {
                            EndGame(); 
                        }
                    }
                }
            }
        }
        public static void EndGame()
        {
            var sortedPlayers = PlayerList.playerIPList
            .OrderBy(player => player.PlayersCards.Count)
            .ToList();
            WinnerList.AddRange(sortedPlayers);

            SendScoreboard();

            GlobalSetting.RestartGame();
        }
    }    
}
