using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace matura
{
    internal class Player
    {
        public string IPAddress;
        public List<Card> PlayersCards;
        public string PlayerName;
        public Player(string ipaddress, List<Card> playercards, string playername) //konstruktor int playernumber,
        {
            IPAddress = ipaddress;
            PlayersCards = playercards;
            PlayerName = playername;
        }
        public override string ToString() //přeformátování pro výpis, aby to bylo srozumitelny - porad moc nechapu proc to nejde bez toho
        {
            return $"{IPAddress}  {PlayersCards}  {PlayerName}";//{PlayerNumber}
        }
    }
    internal class Bot : Player
    {   
        
        public Bot(List<Card> playercards, string playername) : base("BOT", playercards, playername) //ten konstruktor si vezme věci z konstruktoru od hráče https://learn.microsoft.com/cs-cz/dotnet/csharp/fundamentals/tutorials/inheritance
        {
           
        }
        
        List<Card> playableCards = new List<Card>();
        public int BotPlayCard(Bot bot, Card cardontop)
        {
            Console.WriteLine("Volám BotPlayCard...");
            playableCards.Clear(); //myslim že neni potřeba, ale radeji to tu necham

            //Thread.Sleep(5000);
            Console.WriteLine("botovi karty:");
            foreach (Card card in bot.PlayersCards)
            {
                Console.WriteLine($"{card}");
                
                //poslední eso, co platí
                if (cardontop.CardValue == "Eso" && Game.AceFactor == true)
                {                    
                    if (card.CardValue == "Eso")
                    {
                        playableCards.Add(card);
                    }                    
                }

                //poslední 7, co platí
                else if (cardontop.CardValue == "7" && Game.SevenCount > 0)
                {                    
                    if (card.CardValue == "7")
                    {
                        playableCards.Add(card);
                    }                    
                }

                //poslední cokoli - zahraje cokoli (kromě svrška)
                else
                {
                    if (cardontop.CardValue == "svršek") //poslední karta byla svršek
                    {                        
                        if (!string.IsNullOrEmpty(Game.Color) && card.CardColor == Game.Color && card.CardValue != "svršek")
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
        public int BotPlayCardIndex(Bot bot, Card cardontop)
        {


            if (playableCards.Count > 0)
            {
                Random rnd = new Random();
                int CardCount = playableCards.Count;
                int CardIndex = rnd.Next(0, CardCount);

                Card playcard = playableCards[CardIndex];
                int PlayCardIndex = bot.PlayersCards.FindIndex(0, PlayersCards.Count, card => card == playcard); // chat
                return PlayCardIndex + 1; //myslim ze u hrace tam je pro zahrání -1, tak at se to da napojit na ten kod
            }
            else
            {
                // ještě kontrola svršků
                foreach (Card card in bot.PlayersCards)
                {
                    if (card.CardValue == "svršek" && Game.SevenCount <= 0 &&  Game.AceFactor != true) //svršci jdou jedině, poud neni 7 ani eso cardontop.CardValue != "7" && cardontop.CardValue != "Eso" &&
                    {
                        Game.Color = GetMostFrequentColor(bot);

                        Console.WriteLine($"zahrál svrška a změnil na: {Game.Color}");

                        Game.GameInfo = $"zahrál svrška a změnil na {Game.Color}";
                        Game.QueenFactor = false;
                        Game.ChangedBotColor = true;
                        
                        playableCards.Add(card);                    
                    }
                }
                if (playableCards.Count > 0)
                {
                    return BotPlayCardIndex(bot, cardontop);
                }
                else 
                { 
                    return 0;
                }                             
            }
        }
        List<string> CardsColor = new List<string>();
        string GetMostFrequentColor(Bot bot)
        {
            CardsColor.Clear();

            foreach (var card in bot.PlayersCards)
            {
                if (card.CardValue != "svršek") //nechci počítat svršky, nebo to co zahraju (pořád svršek)
                {
                    CardsColor.Add(card.CardColor);

                }
            }
            var mostFrequentColor = CardsColor
                .GroupBy(color => color)                // Seskupíme podle barvy
                .OrderByDescending(group => group.Count()) // Seřadíme podle počtu výskytů
                .FirstOrDefault()?.Key;                 // Vezmeme barvu s největším výskytem

            return mostFrequentColor ?? RandomColor(); //?? pokud je to ředchozí null, tak se použije to další


        }
        string RandomColor()
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
                int CardIndex = rnd.Next(0, 3);
                
                string ChoseColor = PackofCards.CardClolor[CardIndex];
                return ChoseColor;
            }
        }                
    }
}
