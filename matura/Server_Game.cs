using System.Net;
using System.Net.Sockets;
using System.Text;

namespace matura
{
    internal class Server_Game
    {
        public static int sevenCount = 0;
        public static bool aceFactor = false;
        public static bool queenFactor = false;

        public static string selectedColor = "";
        private static string aceMessage = "";
        private static string sevenMessage = "";

        public static bool moreThenOnePlayer = true;
        public static string gameInfo = "";
        private static Card playedCard = null!;
        public static bool botChangedColor = false;
        private static string colorInfo = "";
        private static Player? playerOnTurn;
        private static string messageToWinner = "VYHRÁL jsi";
        private static string messageToAll = "VYHRÁL";
        private static string message = "";

        private static List<Player> winnerList = new List<Player>();
        private static List<Player> toKickList = new List<Player>();
        
        public static void Game()
        {   
            if (GlobalSetting.serverAndPlayerOnOneDevice == false)
            {
                Console.WriteLine("\nNové kolo\n");
            }
            
            Thread ReconnectThread = new Thread(ReturnToGame);
            ReconnectThread.Start();

            if (GlobalSetting.serverAndPlayerOnOneDevice == false)
            {
                Thread keyThread = new Thread(KeyPress); 
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
             playerOnTurn = player;
            
            if (toKickList.Contains(player)) //pokud byl odebrán již v průběhu kola, tak se přeskočí
            {
                return;
            }

            Card lastcard = PackofCards.discardpile.Last();            

            string WhoIsOnTurn = $"{player.playerName}.onturn";
            SendToAll(WhoIsOnTurn);

            int cardCount = player.playersCards.Count;

            string handCards = string.Join("\n     ", player.playersCards.Select((card, index) => $"{index + 1}: {card}")); //chatGPT mi řekl, jak použít string.join
            colorInfo = "";

            string lastCardString = lastcard.ToString();
            if (!string.IsNullOrEmpty(selectedColor))
            {
                colorInfo = $" -> barva je: {selectedColor}";
            }

            message = $"Na vrchu balíčku: {lastCardString}{colorInfo}{aceMessage}{sevenMessage}|" +
                $"\n     Karty v ruce:" +
                $"\n     {handCards}" +
                $".{cardCount}";

            FirstInfo(lastcard, colorInfo); //aby se hráčům vyplňila tabulka
            SendMessage(message, player);

            GettingResponse(player, lastcard);
        }
        private static void GettingResponse(Player player,Card lastcard)
        { 
            int response;
            
            do
            {
                if (GlobalSetting.serverAndPlayerOnOneDevice == false)
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

                bool NewTry = CheckGameLogic(lastcard, player, response); 
             
                if (queenFactor == true && player.IPEndPoint != null)
                {
                    QueenEffect(player);
                }
                else queenFactor = false;
                
                if (NewTry == true)
                {
                    string Message2 = "Toto nemůžeš, zkus to znova";
                    SendMessage(Message2, player);
                }
                else 
                {
                    colorInfo = "";
                    SendGameInfo(player, gameInfo);
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

            if (sevenCount > 0)
            {
                NumberOfCards = sevenCount * 2;
                sevenMessage = " -> na tebe neplatí";
                sevenCount = 0;
            }

            if (aceFactor == true)
            {
                gameInfo = "stál";
                aceMessage = " -> někdo jiný už stál";
                aceFactor = false;
            }
            else
            {
                gameInfo = $"si {NumberOfCards} krát lízl ";
                PackofCards.DrawCard(player, NumberOfCards);
            }           

            SendGameInfo(player, gameInfo);
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
            selectedColor = GetCardColor(responseColor);
            
            gameInfo = $"zahrál svrška a změnil na {selectedColor}";

            queenFactor = false;
        }
        private static bool CheckGameLogic(Card CardonDeck, Player player, int CardonHandIndex)
        {
            CardonHandIndex--;
            playedCard = player.playersCards[CardonHandIndex];
            if (botChangedColor == false)
            {
                gameInfo = $"zahrál {playedCard}";
            }

            //na vrchu 7 

            if (CardonDeck.cardValue == "7" && sevenCount > 0)  //první sedma neplatí
            {
                aceMessage = "";
                if (playedCard.cardValue == "7")
                {
                    sevenCount++;
                    sevenMessage = $" -> musíš zahrát 7, nebo si líznout {2 * sevenCount}";
                    PackofCards.PlayCard(CardonHandIndex, player);

                    return false;
                }
                else
                {
                    return true;
                }
            }

            // na vrchu eso

            else if (CardonDeck.cardValue == "Eso" && aceFactor == true) //první eso neplatí
            {
                sevenMessage = "";
                if (playedCard.cardValue == "Eso")
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
                aceMessage = ""; 
                sevenMessage = "";

                if (!string.IsNullOrEmpty(selectedColor) && botChangedColor == false)  //když se hraje na svrška, tak se nesmí jeho barva, ale jenom ta zvolenou
                {
                    if (playedCard.cardColor == selectedColor || playedCard.cardValue == "svršek") //na svrška se smí dát svršek
                    {
                        PackofCards.PlayCard(CardonHandIndex, player);
                        PlayedCardProperty();
                        selectedColor = "";
                        
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else if (botChangedColor == true) //pokud je na tahu bot, tak si uz vybral barvu a tady se to přeskočí
                {
                    botChangedColor = false;

                    PackofCards.PlayCard(CardonHandIndex, player);
                    PlayedCardProperty();
                    return false;
                }
                else //pokud nebyl svršek
                {
                    if (playedCard.cardColor == CardonDeck.cardColor || playedCard.cardValue == CardonDeck.cardValue || playedCard.cardValue == "svršek")
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
            if (playedCard.cardValue == "7") //sedmička při rozdání se nepočítá
            {
                sevenCount++;
                sevenMessage = $" -> musíš zahrát 7, nebo si líznout {2 * sevenCount}";
            }
            else { sevenCount = 0; }
            if (playedCard.cardValue == "Eso") //eso při rozdání se nepočítá
            {
                aceFactor = true;
                aceMessage = " -> musíš stát (stiskni 0), nebo zahrát eso";
            }
            else { aceFactor = false; } 
            if (playedCard.cardValue == "svršek") //svršek při rozdání se nepočítá
            {
                queenFactor = true;
            }
           
            if (GlobalSetting.serverAndPlayerOnOneDevice == false)
            {
                Console.WriteLine($"je změněno na {selectedColor}, svršek je {playedCard.cardColor}");
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
            if (player.playersCards.Count == 0)
            {
                if (GlobalSetting.serverAndPlayerOnOneDevice == false)
                {
                    Console.WriteLine($"{player.playerName} Dohrál");
                }

                winnerList.Add(player);

                SendMessage(messageToWinner, player);
                messageToWinner = "DOHRÁL jsi";

                string messagetosend = $"{player.playerName} {messageToAll}";
                SendToAll(messagetosend);
                messageToAll = "DOHRÁL";
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
                if (GlobalSetting.serverAndPlayerOnOneDevice == false)
                {
                    Console.WriteLine($"Odesílám: {Message}");
                }
            }
            catch (Exception ex)
            {
                if (GlobalSetting.serverAndPlayerOnOneDevice == false)
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
                    if (player.IPEndPoint == null || toKickList.Contains(player))
                    {
                        return -1;
                    }

                    IPEndPoint responseEndPoint = new IPEndPoint(player.IPEndPoint.Address, GlobalSetting.serverPort);

                    byte[] receivedData = Server_Server.udpClient.Receive(ref responseEndPoint);
                    response = int.Parse(Encoding.UTF8.GetString(receivedData));
                   
                    stillreceive = false;
                }
                catch (SocketException e) when (e.SocketErrorCode == SocketError.TimedOut)
                {

                }
                catch (Exception ex)
                {
                    if (GlobalSetting.serverAndPlayerOnOneDevice == false)
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
                int cardCount = player.playersCards.Count;
                OthersCards += $"\n{player.playerName},{cardCount}";
            }
            OthersCards += "\n";
            
            string GameInfoToSend = $"{PlayerOnTurn.playerName} -> {GameInfo}{colorInfo}.{OthersCards}.informace";
            SendToAll(GameInfoToSend);
        }
        public static void SendToAll(string MessageToAll)
        {
            foreach (var player in PlayerList.playerIPList)
            {
                SendMessage(MessageToAll, player);
            }
            foreach (var player in winnerList)
            {
                SendMessage(MessageToAll, player);
            }
        }
        private static void RemoveWinners() //or kicked players
        {
            foreach (var player in PlayerList.playerIPList.ToList())
            {
                if (winnerList.Contains(player) || toKickList.Contains(player))
                {
                    PlayerList.playerIPList.Remove(player);
                }
            }
            if (PlayerList.playerIPList.Count < 2)
            {
                if (PlayerList.playerIPList.Count == 1)
                {
                    winnerList.Add(PlayerList.playerIPList[0]);
                }

                SendScoreboard();

                moreThenOnePlayer = false;
                if (GlobalSetting.serverAndPlayerOnOneDevice == false) GlobalSetting.RestartGame();
            }
        }
        private static void SendScoreboard()
        {
            int number = 1;
            string MessageToAll = "Scoreboard:";
                        
            var sortedWinners = winnerList.OrderBy(p => p.playersCards.Count).ToList(); //seřadí se podle počtu karet, ti co vyhráli, mají nula a napíšou se tak, jak tam jsou

            for (int i = 0; i < sortedWinners.Count; i++)
            {
                if (i > 0 && sortedWinners[i].playersCards.Count != sortedWinners[i - 1].playersCards.Count && sortedWinners[i].playersCards.Count != 0 || i > 0 && sortedWinners[i].playersCards.Count == 0) //hráči s jiným počtem karet jdou na další pozici
                {
                    number++ ; 
                }

                Console.WriteLine($"{number}. {sortedWinners[i].playerName}");
                
                MessageToAll += $"\n{number}. {sortedWinners[i].playerName}";
            }

            foreach (var PlayerWinner in winnerList)
            {
                SendMessage(MessageToAll, PlayerWinner);
            }
        }
        private static void FirstInfo(Card lastcard, string ColorInfo)
        {
            string OthersCards = "";
            foreach (var player in PlayerList.playerIPList)
            {
                int cardCount = player.playersCards.Count;
                OthersCards += $"\n{player.playerName},{cardCount}"; 
            }
            foreach (var player in PlayerList.playerIPList)
            {
                string handCards = string.Join("\n     ", player.playersCards.Select((card, index) => $"{index + 1}: {card}")); 

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
                    Byte[] receiveBytes = Server_Server.returnUdpClient.Receive(ref IpEndPoint); 

                    string returnData = Encoding.UTF8.GetString(receiveBytes); 

                    string[] parts = returnData.Split('.');
                    string PlayerName = parts[1];
                    
                    if (returnData.Contains("LOOKINGFORSERVER") && PlayerList.playerIPList.Any(player => player.playerName == PlayerName && player.IPEndPoint != null && player.IPEndPoint.Address.Equals(IpEndPoint.Address))) //je tam ta kontrola stejné ip, aby se nikdo nepřihlásil za někoho jiného
                    {
                        if (GlobalSetting.serverAndPlayerOnOneDevice == false)
                        {
                            Console.WriteLine($"Znovu nalezen hráč: {IpEndPoint}");
                        }
                        var player = PlayerList.playerIPList.FirstOrDefault(p => p.playerName == PlayerName && p.IPEndPoint?.Address.Equals(IpEndPoint.Address) == true); //to rovna se true je: pokud IPEndPoint neni null, tak to jde dál
                        if (player != null && player.IPEndPoint != null)  
                        {
                            player.IPEndPoint.Port = IpEndPoint.Port;
                            Console.WriteLine($"Port hráče {PlayerName} byl změněn na {IpEndPoint.Port}");

                            string response = "MAUMAUSERVER";
                            SendMessage(response, player);
                        }
                        if (playerOnTurn == player && player != null)
                        {
                            Thread.Sleep(200);
                            SendMessage(message, player);
                        }
                    }
                    else if (returnData.Contains("KICKME"))
                    {
                        var playertokick = PlayerList.playerIPList.FirstOrDefault(p => p.playerName == PlayerName);
                        if (playertokick != null)
                        {
                            if (playertokick.playersCards.Count > 0)
                            {
                                PackofCards.deck.AddRange(playertokick.playersCards); // vrátí se jeho karty
                            }

                            toKickList.Add(playertokick); //mažou se až na konci kola

                            string response = "YOUWEREKICKED";
                            byte[] responseData = Encoding.UTF8.GetBytes(response);
                            Server_Server.returnUdpClient.Send(responseData, responseData.Length, IpEndPoint);
                            Console.WriteLine($"{playertokick.playerName} opustil hru");

                            string info = "Opustil hru";
                            SendGameInfo(playertokick, info);
                        }
                    }
                    else
                    {
                        string response = "WRONGNAME";
                        byte[] responseData = Encoding.UTF8.GetBytes(response);
                        Server_Server.returnUdpClient.Send(responseData, responseData.Length, IpEndPoint);
                    }
                }
                catch (SocketException e) when (e.SocketErrorCode == SocketError.TimedOut)
                {

                }
                catch (Exception ex)
                {
                    if (GlobalSetting.serverAndPlayerOnOneDevice == false)
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
                Console.WriteLine($"- {p.playerName}");
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
                    Player? playerToKick = PlayerList.playerIPList.FirstOrDefault(p => p.playerName == playertokick);

                    if (playerToKick == null)
                    {
                        Console.WriteLine($"Hráč {playertokick} nebyl nalezen.");
                    }
                    else
                    {
                        Console.WriteLine("Jsi si opravdu jistý? Pokud ano, stiskni Enter.");
                        if (Console.ReadKey(true).Key == ConsoleKey.Enter)
                        {
                            if (playerToKick.playersCards.Count > 0)
                            {
                                PackofCards.deck.AddRange(playerToKick.playersCards); // vrátí se jeho karty
                            }
                                                        
                            toKickList.Add(playerToKick); //mažou se až na konci kola

                            string info = "Byl vyhozen ze hry";
                            SendGameInfo(playerToKick, info);
                            string message = "YOUWEREKICKED";
                            SendMessage(message, playerToKick);
                            done = true;
                        }
                    }
                }

            } while (done == false);
            Console.WriteLine($"Hráč byl vyhozen."); //tady jsi měl cyhbu (potom)
        }
        private static void KeyPress()
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
                        Console.WriteLine("Jsi si opravdu jistý? Pokud ano, stiskni Enter.");
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
            .OrderBy(player => player.playersCards.Count)
            .ToList();
            winnerList.AddRange(sortedPlayers);

            SendScoreboard();

            GlobalSetting.RestartGame();
        }
    }    
}
