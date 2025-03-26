using System.Net;

namespace matura
{
    internal class PlayerList
    {
        public static List<Player> playerIPList = new List<Player>(); 
        private static int BotsNumber = 1;
        
        public static void AddPlayer(IPEndPoint PlayerIPEndPoint, string PlayerName) 
        {
            if (!playerIPList.Any(player => player.PlayerName == PlayerName)) 
            {
                List<Card> PlayersCards = new List<Card>(); 
                
                Player newPlayer = new Player(PlayerIPEndPoint, PlayersCards, PlayerName); 
                playerIPList.Add(newPlayer);

                int cursorLeft = Console.CursorLeft;
                int cursorTop = Console.CursorTop;

                Console.SetCursorPosition(0, Console.WindowTop + Console.WindowHeight - 1); // chatGPT 
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Přidal se: hráč se jménem: {newPlayer.PlayerName}".PadRight(Console.WindowWidth));
                Console.ResetColor();
                Console.SetCursorPosition(cursorLeft, cursorTop); // chatGPT 
                
                               
                string Message = $"připojil se do hry"; 
                                                                  
                Server_Game.SendGameInfo(newPlayer, Message);
            }
            else
            {
                Server_Server.TakenName = true;
            }
        }
        public static void AddBot()
        {
            List<Card> BotsCards = new List<Card>();

            string BotsName = $"Bot {BotsNumber}";
            Bot newBot = new Bot(BotsCards, BotsName); 

            playerIPList.Add(newBot); 

            if (GlobalSetting.SaPOnOneDevice == false)
            {
                Console.WriteLine($"\nPřidal se bot: {newBot.PlayerName}");
            }

            string Message = $"připojil se do hry";
                        
            Server_Game.SendGameInfo(newBot, Message);

            BotsNumber++;
        }
        public static void PrintPlayerList()
        {
            Console.WriteLine("players:");
            foreach (var player in playerIPList)
            {
                Console.WriteLine("");
                Console.WriteLine($"IP adresa: {player.IPEndPoint} se jménem {player.PlayerName}, má karty:");//{player.PlayerNumber}
                foreach (var card in player.PlayersCards)
                {
                    Console.Write("{0} ", card);
                }
            }
            Console.WriteLine($"počet hráčů: {playerIPList.Count}");
            Console.WriteLine("");
        }
    }
}
