using System.Net;

namespace matura
{
    internal class Player
    {
        public IPEndPoint? IPEndPoint;
        public List<Card> PlayersCards;
        public string PlayerName;

        public Player(IPEndPoint? ipendpoint, List<Card> playercards, string playername)
        {
            IPEndPoint = ipendpoint;
            PlayersCards = playercards;
            PlayerName = playername;
        }
        public override string ToString() 
        {
            return $"{IPEndPoint}  {PlayersCards}  {PlayerName}";
        }
    }
    internal class Bot : Player
    {          
        public Bot(List<Card> playercards, string playername) : base(null, playercards, playername) //https://learn.microsoft.com/cs-cz/dotnet/csharp/fundamentals/tutorials/inheritance
        {
           
        }
        
        List<Card> playableCards = new List<Card>();
        List<string> CardsColor = new List<string>();

        public int BotPlayCard(Bot bot, Card cardontop)
        {
            playableCards.Clear();

            if (GlobalSetting.SaPOnOneDevice == false)
            {
                Console.WriteLine("botovi karty:");
            }
            foreach (Card card in bot.PlayersCards)
            {
                if (GlobalSetting.SaPOnOneDevice == false)
                {
                    Console.WriteLine($"{card}");
                }
                
                //poslední eso, co platí
                if (cardontop.CardValue == "Eso" && Server_Game.AceFactor == true)
                {                    
                    if (card.CardValue == "Eso")
                    {
                        playableCards.Add(card);
                    }                    
                }

                //poslední 7, co platí
                else if (cardontop.CardValue == "7" && Server_Game.SevenCount > 0)
                {                    
                    if (card.CardValue == "7")
                    {
                        playableCards.Add(card);
                    }                    
                }

                //poslední cokoli - zahraje cokoli (kromě svrška)
                else
                {
                    if (cardontop.CardValue == "svršek") 
                    {                        
                        if (!string.IsNullOrEmpty(Server_Game.Color) && card.CardColor == Server_Game.Color && card.CardValue != "svršek")
                        {
                            playableCards.Add(card);
                        }
                    }
                    else
                    {
                        if (cardontop.CardValue == card.CardValue && card.CardValue != "svršek" || cardontop.CardColor == card.CardColor && card.CardValue != "svršek")
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
                int PlayCardIndex = bot.PlayersCards.FindIndex(0, PlayersCards.Count, card => card == playcard); // funkci mi ukazal chatGPT
                return PlayCardIndex + 1; //u hrace tam je pro zahrání -1, tak at se to da napojit na ten kod
            }
            else
            {
                // kontrola svršků - pokud bot neměl co zahrát, tak zkusí jestli nemá svrška
                foreach (Card card in bot.PlayersCards)
                {
                    if (card.CardValue == "svršek" && Server_Game.SevenCount <= 0 &&  Server_Game.AceFactor != true) //svršci jdou jedině, poud neni 7 ani eso
                    {
                        Server_Game.Color = GetMostFrequentColor(bot);

                        if (GlobalSetting.SaPOnOneDevice == false)
                        {
                            Console.WriteLine($"zahrál svrška a změnil na: {Server_Game.Color}");
                        }
                        Server_Game.GameInfo = $"zahrál svrška a změnil na {Server_Game.Color}";
                        Server_Game.QueenFactor = false;
                        Server_Game.ChangedBotColor = true;
                        
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
            CardsColor.Clear();

            foreach (var card in bot.PlayersCards)
            {
                if (card.CardValue != "svršek") //vybere barvu z karet, co nejsou svršci
                {
                    CardsColor.Add(card.CardColor);
                }
            }
            var mostFrequentColor = CardsColor
                .GroupBy(color => color)                // Seskupíme podle barvy
                .OrderByDescending(group => group.Count()) // Seřadíme podle počtu výskytů
                .FirstOrDefault()?.Key;                 // Vezmeme barvu s největším výskytem - poradil chatGPT

            return mostFrequentColor ?? RandomColor();
         }
        private string RandomColor()
        {
            if (CardsColor.Count > 0)
            {
                Random rnd = new Random();
                int ColorCount = CardsColor.Count;
                int CardIndex = rnd.Next(0, ColorCount);

                string ChoseColor = CardsColor[CardIndex];
                return ChoseColor;
            }
            else 
            {
                Random rnd = new Random();
                int CardIndex = rnd.Next(0, 4);
                
                string ChoseColor = PackofCards.CardClolor[CardIndex];
                return ChoseColor;
            }
        }                
    }
}
