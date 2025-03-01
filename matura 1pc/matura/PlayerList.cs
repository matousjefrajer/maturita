using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace matura
{
    internal class PlayerList
    {
        public static List<Player> playerIPList = new List<Player>();  // Seznam IP adres připojených hráčů https://learn.microsoft.com/cs-cz/dotnet/api/system.collections.generic.list-1?view=net-8.0
        static int BotsNumber = 1;
        
        public static void AddPlayer(string PlayerIP, string PlayerName) 
        {
            if (!playerIPList.Any(player => player.IPAddress == PlayerIP)) //pokud uz ip adresa je zapsána, tak to neazpíše znovu
            {
                List<Card> PlayersCards = new List<Card>(); //vytvořim mu list na karty
                
                Player newPlayer = new Player(PlayerIP, PlayersCards, PlayerName); // vytvoření hráče
                playerIPList.Add(newPlayer); // Přidání hráče do seznamu
                Console.WriteLine($"vytvořil se: {newPlayer.IPAddress} se jménem {newPlayer.PlayerName}");
                
                string Message = $"připojil se do hry"; //se připojil do hry, kde je: 
                                                                  
                Game.SendGameInfo(newPlayer, Message);
            }
        }
        public static void AddBot()
        {
            List<Card> BotsCards = new List<Card>(); //vytvořim mu list na karty

            string BotsName = $"Bot {BotsNumber}";
            Bot newBot = new Bot(BotsCards, BotsName); // vytvoření bota

            playerIPList.Add(newBot); // Přidání hráče do seznamu
            Console.WriteLine($"\nvytvořil se bot: {newBot.IPAddress} se jménem {newBot.PlayerName}");

            string Message = $"připojil se do hry";//bot se připojil do hry, kde je: 
            foreach (var player in playerIPList)
            {
                Message += $"{player.PlayerName}, ";
            }
            
            Game.SendGameInfo(newBot, Message);

            BotsNumber++;
        }
        public static void PrintPlayerList()
        {
            Console.WriteLine("players:");
            foreach (var player in playerIPList)
            {
                Console.WriteLine("");
                Console.WriteLine($"IP adresa: {player.IPAddress} se jménem {player.PlayerName}, má karty:");//{player.PlayerNumber}
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
