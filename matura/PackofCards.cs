using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace matura
{    internal class PackofCards
    {
        public static List<Card> deck = new List<Card>(); // list pro uložení karet
        public static List<Card> discardpile = new List<Card>(); // list pro uložení karet
        public static Random random = new Random(); // generátor náhodných čísel
        public PackofCards()
        {
            string[] CardClolor = { "Srdce", "Listy", "Koule", "Žaludy" };  // barvy
            string[] CardValue = { "7", "8", "9", "10", "spodek", "svršek", "Král","Ace" };  // bodnoty

            foreach (var cardcolor in CardClolor)// přidání karet do balíčku
            {
                foreach (var cardvalue in CardValue)
                {
                    deck.Add(new Card(cardcolor, cardvalue)); //přidam karty, co mají nějakou barvu a hodnotu
                }
            }
            //Console.WriteLine("before");
            //PrintDeck();
            //Console.WriteLine("after");
            ShuffleDeck(deck);
            PrintDecks();
        }
        
        public static void PrintDecks()// vypsání balíčku
        {
            Console.WriteLine($"balíček");
            foreach (var card in deck)
            {
                Console.WriteLine(card);
            }
            Console.WriteLine($"");

            Console.WriteLine($"odhazovací:");
            foreach (var card in discardpile)
            {
                Console.WriteLine(card); 
            }
        }
        public static void ShuffleDeck(List<Card> deck)
        {
            for (int i = deck.Count - 1; i > 0; i--) //https://stackoverflow.com/questions/273313/randomize-a-listt
            {
                
                int j = random.Next(i + 1);  // náhodné číslo od 0 do i
               
                Card temp = deck[i];
                deck[i] = deck[j];
                deck[j] = temp;
            }
        }

        public static void DrawCard(Player player, int nuberofcards) //nic lepsiho než to nebo to mě nenapadlo Player playerFromclass = null,int PlayerNumber = 100
        {
            //if (deck.Count == 0)
            //{
            //    Console.WriteLine("There is no card, but the game can continue."); //jenom aby to lidi nevyděsilo, že nic nedostaanou
            //    return;
            //}

            //Card card = deck[0];
            //if (playerFromclass == null) 
            //{
            //    Player player = PlayerList.playerIPList[PlayerNumber];
            //    player.PlayersCards.Add(PackofCards.deck[0]);
            //
            //    deck.RemoveAt(0); //odstraní jí a ostatní se posunou
            //}
            CheckDeck(nuberofcards);
            if (deck.Count > 0) // at to nevyhazuje zbytecne chyby
            {

                for (int i = 0; i < nuberofcards; i++)
                {
                    player.PlayersCards.Add(PackofCards.deck[0]);               //musim sem dat ten checkdeck

                    deck.RemoveAt(0); //odstraní jí a ostatní se posunou
                }
            }
            else { Console.WriteLine("neni co"); }
            
            


        }
        public static void PlayCard(int cardindex, Player player)
        {
            //cardindex--;
            Card playedCard = player.PlayersCards[cardindex];
            //Console.WriteLine($"spustilo se to");

            player.PlayersCards.RemoveAt(cardindex);
            discardpile.Add(playedCard);

            Console.WriteLine($"karta, co se stěhuje:");
            Console.WriteLine($"{playedCard}");

            PrintDecks();

        }
        public static void DealCards()
        {
            PlayerList.PrintPlayerList();
           


            int round = 0;
            int WhichPlayer;
            int PlayerCount = PlayerList.playerIPList.Count;
            Console.WriteLine(PlayerCount);

            while (round < 4) // 4 kola
            {
                for (WhichPlayer = 0; WhichPlayer < PlayerCount; WhichPlayer++) //inicializátor, podmínka, iterátor = před, podmínkia po - https://learn.microsoft.com/cs-cz/dotnet/csharp/language-reference/statements/iteration-statements#code-try-4
                {
                    Player player = PlayerList.playerIPList[WhichPlayer];
                    DrawCard(player, 1); // Rozdá hráči kartu (pořadí hráčů se bere podle pořadí v listu
                }
                round++;
            }
            
            Card firstindeck = deck.First();
            deck.RemoveAt(0);
            
            discardpile.Add(firstindeck);
            PrintDecks();


            /*
            int number = 0;
            PackofCards.DrawCard(number);
            int number2 = 1;
            PackofCards.DrawCard(number2);
            int number3 = 2;
            PackofCards.DrawCard(number3);
            int number4 = 0;
            PackofCards.DrawCard(number4);
            int number24 = 1;
            PackofCards.DrawCard(number24);
            int number34 = 2;
            PackofCards.DrawCard(number34);
            int number44 = 0;
            PackofCards.DrawCard(number44);
            int number25 = 1;
            PackofCards.DrawCard(number25);
            int number35 = 2;
            PackofCards.DrawCard(number35);*/
        }
        public static void CheckDeck(int WantredCards) 
        { 
            if (WantredCards >= deck.Count) //když v balíčku méně karet než chci líznout a nezbyde tam ta svrchní, tak to otočí odkládací
            {
                
                Card LastCard = discardpile.Last();
                discardpile.RemoveAt(discardpile.Count - 1); //začíná od 0
                //ShuffleDeck(discardpile);

                deck.AddRange(discardpile); //https://learn.microsoft.com/cs-cz/dotnet/api/system.collections.generic.list-1.addrange?view=net-8.0
                                            // vezmu ty karty z odhazovacího adám je do normálního balíčku
                discardpile.Clear();
                discardpile.Add(LastCard);


                Console.WriteLine("");
                Console.WriteLine("");
                Console.WriteLine("otáčí se balíček");
                Console.WriteLine("");
                Console.WriteLine("");
                PrintDecks();
                
                if (deck.Count == 0) 
                {
                    Console.WriteLine("this player was lucky, there is no card available"); // nejak to oznam tomu hráči
                }



            }
        }

    }
    
}
