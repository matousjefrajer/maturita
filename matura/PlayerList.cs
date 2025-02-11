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
        
        //static int PlayerIndex = 1;
        public static void AddPlayer(string PlayerIP) 
        {
            //List<Card> PlayersCards = new List<Card>();
            if (!playerIPList.Any(player => player.IPAddress == PlayerIP)) //pokud uz ip adresa je zapsána, tak to neazpíše znovu
            {
                List<Card> PlayersCards = new List<Card>(); //vytvořim mu list na karty
                //PlayersStartingCards();
                playerIPList.Add(new Player(PlayerIP,  PlayersCards));//PlayerIndex,
                //PlayerIndex++;

                //PrintPlayerList();
                
            }
        }
        public static void PrintPlayerList()
        {
            Console.WriteLine("players:");
            foreach (var player in playerIPList)
            {
                Console.WriteLine("");
                Console.WriteLine($"IP adresa: {player.IPAddress}, má karty:");//{player.PlayerNumber}
                foreach (var card in player.PlayersCards)
                {
                    Console.Write("{0} ", card);
                }
            }
            Console.WriteLine($"počet hráčů: {playerIPList.Count}");
            Console.WriteLine("");

        }
        /*
        static void PlayersStartingCards() 
        {
            PlayersCards.AddRange(PackofCards.deck.GetRange(0, 4));
            //PlayersCards.Add(PackofCards.deck[1]);
            //PlayersCards.Add(PackofCards.deck[2]);
            //PlayersCards.Add(PackofCards.deck[3]);
        }*/
       
        /*
        public List<Player> GetPlayerList() //nejspis nepouziju nakonec
        {
            return playerIPList;
        }*/
    }
}
