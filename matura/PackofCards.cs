namespace matura
{    internal class PackofCards
    {
        public static List<Card> deck = new List<Card>(); 
        public static List<Card> discardpile = new List<Card>(); 
        public static Random random = new Random();

        public static string[] CardClolor = { "Srdce", "Listy", "Kule", "Žaludy" };  
        private static string[] CardValue = { "7", "8", "9", "10", "spodek", "svršek", "Král", "Eso" };  
        public PackofCards()
        {           
            foreach (var cardcolor in CardClolor)
            {
                foreach (var cardvalue in CardValue)
                {
                    deck.Add(new Card(cardcolor, cardvalue)); 
                }
            }
            
            ShuffleDeck(deck);
        }
        public static void PrintDecks()
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
        private static void ShuffleDeck(List<Card> deck) //chatGPT
        {
            for (int i = deck.Count - 1; i > 0; i--) 
            {
                int j = random.Next(i + 1); 
               
                Card temp = deck[i];
                deck[i] = deck[j];
                deck[j] = temp;
            }
        }
        public static void DrawCard(Player player, int nuberofcards) 
        {            
            CheckDeck(nuberofcards);
            
            for (int i = 0; i < nuberofcards && deck.Count > 0; i++)
            {                
                player.PlayersCards.Add(PackofCards.deck[0]); 
                                
                deck.RemoveAt(0);
            }
            if (deck.Count == 0) 
            {
                Server_Game.GameInfo += "je prázdný balíček a již neni co lízat";
            }
        }
        public static void PlayCard(int cardindex, Player player)
        {
            Card playedCard = player.PlayersCards[cardindex];
            
            player.PlayersCards.RemoveAt(cardindex);
            discardpile.Add(playedCard);
        }
        public static void DealCards()
        {
            int round = 0;
            int WhichPlayer;
            int PlayerCount = PlayerList.playerIPList.Count;
            
            while (round < 4) // 4 kola, protože se rozdává po 4
            {
                for (WhichPlayer = 0; WhichPlayer < PlayerCount; WhichPlayer++)
                {
                    Player player = PlayerList.playerIPList[WhichPlayer];
                    DrawCard(player, 1);
                }
                round++;
            }
            
            Card firstindeck = deck.First();
            deck.RemoveAt(0);
            
            discardpile.Add(firstindeck);

            if (GlobalSetting.SaPOnOneDevice == false)
            {
                PlayerList.PrintPlayerList();
            }

            string message = "Rozdaly se karty";
            Server_Game.SendToAll(message);
        }
        private static void CheckDeck(int WantredCards) 
        { 
            if (WantredCards >= deck.Count) //když v balíčku méně karet než chci líznout a nezbyde tam ta svrchní, tak to otočí odkládací
            {                
                Card LastCard = discardpile.Last();
                discardpile.RemoveAt(discardpile.Count - 1); 
                ShuffleDeck(discardpile); 

                deck.AddRange(discardpile); 
                discardpile.Clear();
                discardpile.Add(LastCard);

                if (GlobalSetting.SaPOnOneDevice == false)
                {
                    Console.WriteLine("otáčí se balíček");
                }
                
                if (deck.Count == 0) 
                {
                    if (GlobalSetting.SaPOnOneDevice == false)
                    {
                        Console.WriteLine("Měl štěstí, protože balíček je prázdný"); 
                    }
                }
                string message = "Otáčí se a míchá odhazovací balíček";
                Server_Game.SendToAll(message);
            }
        }

    }
    
}
