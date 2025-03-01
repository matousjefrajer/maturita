using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace matura
{
    internal class Game
    {
        public static int SevenCount = 0;
        public static bool AceFactor = false;
        public static bool QueenFactor = false;

        public static string Color = "";
        static string AceMessage = "";
        static string SevenMessage = "";

        public static bool MoreThenOnePlayer = true;
        public static string GameInfo = "";
        static Card playedCard = null!; 
        public static bool ChangedBotColor = false;
        static string Message = "";
        static Player Player = null!;
        static string ColorInfo = "";

        static string MessageToWinner = "VYHRÁL jsi";
        static string messagetoall = "VYHRÁL";
        public static List<Player> WinnerList = new List<Player>();

        static int Port = 13000;
        static UdpClient udpClient = new UdpClient();
        public static void game()
        {
            Thread keyThread = new Thread(CheckForKeyPress);
            keyThread.Start();

            foreach (var player in PlayerList.playerIPList)
            {
                Console.WriteLine("\n" +
                    "Nové kolo" +
                    "\n");

                Card lastcard = PackofCards.discardpile.Last(); //poslední karta je žejo na vrchu balíčku
                bool NewTry = true;
                Player = player;  //pro to znovupřipojení              
                string WhoIsOnTurn = $"{player.PlayerName}.onturn";
                SendToAll(WhoIsOnTurn);
                
                int cardCount = player.PlayersCards.Count;
                
                string handCards = string.Join("\n     ", player.PlayersCards.Select((card, index) => $"{index + 1}: {card}")); //chatGPT card.ToString()
                ColorInfo = "";
                
                string lastCardString = lastcard.ToString();
                if (!string.IsNullOrEmpty(Color))
                {
                    ColorInfo = $" -> barva je: {Color}";
                    //Color = null;
                }
                
                Message = $"Na vrchu balíčku: {lastCardString}{ColorInfo}{AceMessage}{SevenMessage}|" +
                    $"\n     Karty v ruce:" +
                    $"\n     {handCards}" +
                    $".{cardCount}";

                FirstInfo(lastcard, ColorInfo); //pro vyplnění té tabulky
                SendMessage(Message, player);
                                                
                do
                {                    
                    Console.WriteLine("Čekám na odpověď...");
                 
                    int response;
                    if (player.IPAddress == "BOT")
                    {
                        //Bot bot = player as Bot; 
                        Bot bot = (Bot)player;//přetypuju
                        response = bot.BotPlayCard(bot, lastcard);
                        
                        //PlayerList.PrintPlayerList();
                    }
                    else
                    {
                        response = ReceveMessage(player);
                    }
                    
                    //Console.WriteLine($"Přijatá odpověď: {response}");

                    if (response == 0)
                    {
                        int NumberOfCards = 1;

                        if (SevenCount > 0) //hraní sedmiček
                        { 
                            NumberOfCards = SevenCount * 2;
                            SevenMessage = " -> na tebe neplatí";
                        } 
                        
                        if (AceFactor == true)
                        {
                            GameInfo = "stál";
                            //Console.WriteLine($"nic nedosatne");
                            AceMessage = " -> někdo jiný už stál = hraj cokoli";
                            AceFactor = false;
                        }
                        else
                        {
                            //Console.WriteLine($"počet sedmiček {SevenCount} , lízané karty {NumberOfCards}");
                            GameInfo = $"si {NumberOfCards} krát lízl ";
                            PackofCards.DrawCard(player, NumberOfCards);
                        }
                        SevenCount = 0;
                                                
                        SendGameInfo(player, GameInfo);
                        break;
                    }

                    NewTry = CheckGameLogick(lastcard, player, response); //kontrola, jestli to jde ztahrát a přpadné počítání sedmiček
                    
                    if (QueenFactor == true && player.IPAddress != "BOT")
                    {
                        string Message3 = "     jakou barvu chceš:" +
                            "\n     1) srdce" +
                            "\n     2) koule" +
                            "\n     3) listy" +
                            "\n     4) žaludy";
                     
                        SendMessage(Message3, player);
                        
                        int responseColor = ReceveMessage(player);
                        //Console.WriteLine($"Přijatá odpověď ohledně barvy: {responseColor}");

                        Color = GetCardColor(responseColor);
                        //Console.WriteLine($"barva: {Color}");

                        GameInfo = $"zahrál svrška a změnil na {Color}";

                        QueenFactor = false;
                    }
                    else
                    {
                        QueenFactor = false;
                    }
                    
                    //Console.WriteLine($"newtry: {NewTry}");
                    if (NewTry == true) 
                    {
                        string Message2 = "Toto nemůžeš, zkus to znova";//uprava odpovědi
                        SendMessage(Message2, player);
                        
                    }
                    else //pokud neni pořeba ho nutit znova odpovídat, tak se to uzavře
                    {
                        SendGameInfo(player, GameInfo);
                        Win(player);
                        break;
                    }
                    
                } while (true);

                //Console.WriteLine("Zahrál");
            }
            RemoveWinners();
        }
        public static bool CheckGameLogick(Card CardonDeck, Player player, int CardonHandIndex)
        {
            CardonHandIndex--;
            playedCard = player.PlayersCards[CardonHandIndex];
            GameInfo = $"zahrál {playedCard}"; //tohle neni nejlogičtější místo, protoře i když se mu to nepodaří zahrát, tak se to přepíše na "zahrál", ale zpráva o zahrání se posílá, až je to schváleno, takže to už by němelo být chybně
            
            //na vrchu 7 

            if (CardonDeck.CardValue == "7" && SevenCount > 0)  //jakoze první sedma neplatí
            {
                AceMessage = ""; //potřebuju, když tam neni eso, at to nic nepáše
                if (playedCard.CardValue == "7")
                {
                    SevenCount++;
                    SevenMessage = $" -> musíš zahrát 7, nebo si líznout {2 * SevenCount}";
                    PackofCards.PlayCard(CardonHandIndex, player);
                    
                    return false;
                }
                else
                {
                    //Console.WriteLine($"false1");
                    return true;
                }
            }
            
            // na vrchu eso

            else if (CardonDeck.CardValue == "Eso" && AceFactor == true) //jakoze první eso neplatí
            {
                SevenMessage = "";
                if (playedCard.CardValue == "Eso")
                {
                    PackofCards.PlayCard(CardonHandIndex, player);
                    
                    return false;
                }
                else
                {
                    //Console.WriteLine($"false2");
                    return true;
                }
            }
            
            //na vrchu cokoli

            else 
            {
                AceMessage = ""; //potřebuju, když tam neni eso, at to nic nepáše
                SevenMessage = "";
                                
                if (!string.IsNullOrEmpty(Color) && ChangedBotColor == false )  //když hraju na svrška, tak nesmim jeho barvu, ale jenom tu zvolenou
                {
                    if (playedCard.CardColor == Color || playedCard.CardValue == "svršek") //na svrška muzes dat svrška
                    {
                        //Console.WriteLine($"Karta je zahraná");
                        PackofCards.PlayCard(CardonHandIndex, player);
                        PlayedCardProperty();
                        Color = "";
                        
                        return false;
                    }
                    else
                    {
                        //Console.WriteLine($"Nepovedlo se");
                        return true;
                    }
                }
                else if (ChangedBotColor == true) 
                { 
                    ChangedBotColor = false;
                   
                    PackofCards.PlayCard(CardonHandIndex, player);
                    PlayedCardProperty();
                    return false;
                }
                else //pokud nebyl svršek, tak to jde normálně
                {
                    if (playedCard.CardColor == CardonDeck.CardColor || playedCard.CardValue == CardonDeck.CardValue || playedCard.CardValue == "svršek")
                    {
                        //Console.WriteLine($"Karta je zahraná");

                        PackofCards.PlayCard(CardonHandIndex, player);
                        PlayedCardProperty();
                        
                        return false;
                    }
                    else
                    {
                        //Console.WriteLine($"Nepovedlo se");
                        return true;
                    }
                }
            }
        }
        static void PlayedCardProperty()
        {
            if (playedCard.CardValue == "7") //sedmička při rozdání se nepočítá
            {
                SevenCount++;
                SevenMessage = $" -> musíš zahrát 7, nebo si líznout {2*SevenCount}";
                //Console.WriteLine($"zahraná sedmička {SevenCount}");
            }
            else { SevenCount = 0; }
            if (playedCard.CardValue == "Eso") //sedmička při rozdání se nepočítá
            {
                AceFactor = true;
                AceMessage = " -> musíš stát (stiskni 0), nebo zahrát eso";
                //Console.WriteLine($"eso");
            }
            else { AceFactor = false; } //myslim, že jhe to navíc
            if (playedCard.CardValue == "svršek") //sedmička při rozdání se nepočítá
            {
                QueenFactor = true;
                //GameInfo =
                //Console.WriteLine($"svršek");
            }
            else { //Color = null;
                   } //myslim, že jhe to navíc
            
            Console.WriteLine($"je změněno na {Color}, svršek je {playedCard.CardColor}");
        }


        static string GetCardColor(int number)
        {
            switch (number)
            {
                case 1: return "Srdce";
                case 2: return "Koule";
                case 3: return "Listy";
                case 4: return "Žaludy";
                default: return "neznámá barva"; // nemělo by nastat
            }
        }
        static void Win(Player player) // zkontoŕoluj
        {
            
            if (player.PlayersCards.Count == 0)
            {
                Console.WriteLine($"{player.PlayerName} Dohrál");
                
                WinnerList.Add(player);

                SendMessage(MessageToWinner, player);
                MessageToWinner = "DOHRÁL jsi";

                string messagetosend = $"{player.PlayerName} {messagetoall}";
                SendToAll(messagetosend);
                messagetoall = "DOHRÁL";
            }
            
        }
        
        static void SendMessage(string Message, Player player)
        {
            try
            {
                if (player.IPAddress == "BOT") //botovi nechci posílat, protože by to vyhazovalo chybu
                {
                    return;
                }
                Thread.Sleep(200); //kvuli sekani
                
                IPEndPoint PlayerOnTurn = new IPEndPoint(IPAddress.Parse(player.IPAddress), Port);

                byte[] sendData = Encoding.UTF8.GetBytes(Message);
                udpClient.Send(sendData, sendData.Length, PlayerOnTurn);
                Console.WriteLine($"Odesílám: {Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chyba: {ex.Message}");
            }
        }
        static int ReceveMessage(Player player) 
        {
            int response;
            try
            {              
                IPEndPoint responseEndPoint = new IPEndPoint(IPAddress.Parse(player.IPAddress), Port);

                byte[] receivedData = udpClient.Receive(ref responseEndPoint);
                response = int.Parse(Encoding.UTF8.GetString(receivedData));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chyba: {ex.Message}");
                response = 0;
            }
            return response;
        }
        public static void SendGameInfo(Player PlayerOnTurn, string GameInfo)
        {
            string OthersCards = ""; //"\n"
            foreach (var player in PlayerList.playerIPList)
            {
                int cardCount = player.PlayersCards.Count;
                OthersCards += $"\n{player.PlayerName},{cardCount}"; //hráč má tolik karet
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
        }
        static void RemoveWinners()
        {
            foreach (var player in PlayerList.playerIPList.ToList())
            {
                if (WinnerList.Contains(player))
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

                int number = 1;
                string MessageToAll = "Scoreboard:";
                foreach (var PlayerWinner in WinnerList)
                {
                    Console.WriteLine($"{number}. {PlayerWinner.PlayerName}\n");//{player.PlayerNumber}

                    MessageToAll += $"\n{number}. {PlayerWinner.PlayerName}"; // Přidání jména hráče do zprávy
                    number++;
                }
                foreach (var PlayerWinner in WinnerList) 
                {
                    SendMessage(MessageToAll, PlayerWinner);
                }
                MoreThenOnePlayer = false;
                Environment.Exit(0);
            }
        }
        static void CheckForKeyPress()
        {
            while (Game.MoreThenOnePlayer)
            {
                ConsoleKeyInfo key = Console.ReadKey();
                if (key.Key == ConsoleKey.S)
                {
                    SendMessage(Message, Player);
                    Console.WriteLine($"Posílám mu podruhé zprávu");
                }
                Thread.Sleep(1000); // zpoždění, aby se CPU nezatěžovalo (doporucil chatgpt - chci prokonzultovat)
            }
        }
        public static void FirstInfo(Card lastcard, string ColorInfo)
        {
            string OthersCards = "";
            foreach (var player in PlayerList.playerIPList)
            {
                int cardCount = player.PlayersCards.Count;
                OthersCards += $"\n{player.PlayerName},{cardCount}"; //hráč má tolik karet
            }
            foreach (var player in PlayerList.playerIPList)
            {
                string handCards = string.Join("\n     ", player.PlayersCards.Select((card, index) => $"{index + 1}: {card}")); //chatGPT card.ToString()
                                
                string message = $"Na vrchu balíčku: {lastcard}{ColorInfo}," +
                    $"\n     Karty v ruce:" +
                    $"\n     {handCards}.{OthersCards}.firstinfo";//{AceMessage}{SevenMessage}
                SendMessage(message, player);
            }
        }
    }
    
}
