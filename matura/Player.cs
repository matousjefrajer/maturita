using System.Net;

namespace matura
{
    internal class Player
    {
        public IPEndPoint? IPEndPoint;
        public List<Card> playersCards;
        public string playerName;

        public Player(IPEndPoint? ipendpoint, List<Card> playercards, string playername)
        {
            IPEndPoint = ipendpoint;
            playersCards = playercards;
            playerName = playername;
        }
        public override string ToString() 
        {
            return $"{IPEndPoint}  {playersCards}  {playerName}";
        }
    }
    internal class Bot : Player
    {          
        public Bot(List<Card> playercards, string playername) : base(null, playercards, playername) //https://learn.microsoft.com/cs-cz/dotnet/csharp/fundamentals/tutorials/inheritance
        {
           
        }
        
        List<Card> playableCards = new List<Card>();
        List<string> cardsColors = new List<string>();

        public int BotPlayCard(Bot bot, Card cardontop)
        {
            playableCards.Clear();

            if (GlobalSetting.serverAndPlayerOnOneDevice == false)
            {
                Console.WriteLine("botovi karty:");
            }
            foreach (Card card in bot.playersCards)
            {
                if (GlobalSetting.serverAndPlayerOnOneDevice == false)
                {
                    Console.WriteLine($"{card}");
                }
                
                //poslední eso, co platí
                if (cardontop.cardValue == "Eso" && Server_Game.aceFactor == true)
                {                    
                    if (card.cardValue == "Eso")
                    {
                        playableCards.Add(card);
                    }                    
                }

                //poslední 7, co platí
                else if (cardontop.cardValue == "7" && Server_Game.sevenCount > 0)
                {                    
                    if (card.cardValue == "7")
                    {
                        playableCards.Add(card);
                    }                    
                }

                //poslední cokoli - zahraje cokoli (kromě svrška)
                else
                {
                    if (cardontop.cardValue == "svršek") 
                    {                        
                        if (!string.IsNullOrEmpty(Server_Game.selectedColor) && card.cardColor == Server_Game.selectedColor && card.cardValue != "svršek")
                        {
                            playableCards.Add(card);
                        }
                    }
                    else
                    {
                        if (cardontop.cardValue == card.cardValue && card.cardValue != "svršek" || cardontop.cardColor == card.cardColor && card.cardValue != "svršek")
                        {
                            playableCards.Add(card);
                        }
                    }
                }
            }
            return BotPlayCardIndex(bot, cardontop);
        }
        private int BotPlayCardIndex(Bot bot, Card cardontop)
        {
            if (playableCards.Count > 0)
            {
                Random rnd = new Random();
                int CardCount = playableCards.Count;
                int CardIndex = rnd.Next(0, CardCount);

                Card playcard = playableCards[CardIndex];
                int PlayCardIndex = bot.playersCards.FindIndex(0, playersCards.Count, card => card == playcard); // funkci mi ukazal chatGPT
                return PlayCardIndex + 1; //u hrace tam je pro zahrání -1, tak at se to da napojit na ten kod
            }
            else
            {
                // kontrola svršků - pokud bot neměl co zahrát, tak zkusí jestli nemá svrška
                foreach (Card card in bot.playersCards)
                {
                    if (card.cardValue == "svršek" && Server_Game.sevenCount <= 0 &&  Server_Game.aceFactor != true) //svršci jdou jedině, poud neni 7 ani eso
                    {
                        Server_Game.selectedColor = GetMostFrequentColor(bot);

                        if (GlobalSetting.serverAndPlayerOnOneDevice == false)
                        {
                            Console.WriteLine($"zahrál svrška a změnil na: {Server_Game.selectedColor}");
                        }
                        Server_Game.gameInfo = $"zahrál svrška a změnil na {Server_Game.selectedColor}";
                        Server_Game.queenFactor = false;
                        Server_Game.botChangedColor = true;
                        
                        playableCards.Add(card);                    
                    }
                }
                if (playableCards.Count > 0)
                {
                    return BotPlayCardIndex(bot, cardontop);
                }
                else 
                { 
                    return 0; //pokud už opravdu nemá co, tak zvolí líznutí
                }                             
            }
        }
        private string GetMostFrequentColor(Bot bot)
        {
            cardsColors.Clear();

            foreach (var card in bot.playersCards)
            {
                if (card.cardValue != "svršek") //vybere barvu z karet, co nejsou svršci
                {
                    cardsColors.Add(card.cardColor);
                }
            }
            var mostFrequentColor = cardsColors
                .GroupBy(color => color)                // Seskupíme podle barvy
                .OrderByDescending(group => group.Count()) // Seřadíme podle počtu výskytů
                .FirstOrDefault()?.Key;                 // Vezmeme barvu s největším výskytem - poradil chatGPT

            return mostFrequentColor ?? RandomColor();
         }
        private string RandomColor()
        {
            if (cardsColors.Count > 0)
            {
                Random rnd = new Random();
                int ColorCount = cardsColors.Count;
                int CardIndex = rnd.Next(0, ColorCount);

                string ChoseColor = cardsColors[CardIndex];
                return ChoseColor;
            }
            else 
            {
                Random rnd = new Random();
                int CardIndex = rnd.Next(0, 4);
                
                string ChoseColor = PackofCards.cardClolor[CardIndex];
                return ChoseColor;
            }
        }                
    }
}
