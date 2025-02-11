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
        static int SevenCount = 0;
        static bool AceFactor = false;
        static bool QueenFactor = false;
        static int RoundNumber = 0;
        static string Color = null;
        public static void game()
        {
            //DealCards();

            //PlayerList.PrintPlayerList();
            //PackofCards.PrintDeck();
            RoundNumber++;



            foreach (var player in PlayerList.playerIPList)
            {
                Card lastcard = PackofCards.discardpile.Last(); //poslední karta je žejo na vrchu balíčku
                try
                {
                    bool NewTry = true;
                    //bool SevenAce = true;

                    //Console.WriteLine("zmackni neco");
                    //Console.ReadKey();
                    int Port = 13000;
                    
                    UdpClient udpClient = new UdpClient();
                    //snažim se to poslat
                    IPEndPoint PlayerOnTurn = new IPEndPoint(IPAddress.Parse(player.IPAddress), Port);

                    int cardCount = player.PlayersCards.Count;
                    //string handCards = string.Join(",", player.PlayersCards.Select(card => card.ToString())); //převedu každý prvek z ruky hráče do řetězce, který kokážu poslat
                    string handCards = string.Join("\n", player.PlayersCards.Select((card, index) => $"{index + 1}: {card}")); //chatGPT card.ToString()
                    string ColorInfo = "";
                    string lastCardString = lastcard.ToString();
                    if (Color != null) 
                    {
                        ColorInfo = $" and color is {Color}";
                        //Color = null;
                    }
                    
                    string Message = $"LastCard:{lastCardString}{ColorInfo}" +
                        $"\nHandCards:" +
                        $"\n{handCards}" +
                        $".{cardCount}";
                    
                        //string Data = $"karta na vrchu balíčku: {lastcard.ToString()}, karty v tvé ruce: ";
                        byte[] sendData = Encoding.UTF8.GetBytes(Message);
                        udpClient.Send(sendData, sendData.Length, PlayerOnTurn);
                        //Console.WriteLine("Zpráva odeslána.");
                    do
                    {
                        //cekam na odpoved
                        Console.WriteLine("Čekám na odpověď...");
                        IPEndPoint responseEndPoint = new IPEndPoint(IPAddress.Parse(player.IPAddress), Port);

                        byte[] receivedData = udpClient.Receive(ref responseEndPoint);
                        int response = int.Parse(Encoding.UTF8.GetString(receivedData));
                        Console.WriteLine($"Přijatá odpověď: {response}");



                        if (response == 0)
                        {
                            int NumberOfCards = 1;

                            if (SevenCount > 0) { NumberOfCards = SevenCount * 2; } //hraní sedmiček
                            //PackofCards.CheckDeck(NumberOfCards); //zjisti jestli funguje

                            Console.WriteLine($"počet sedmiček{SevenCount} , lízané karty {NumberOfCards}");
                            
                            if (AceFactor == true)
                            {
                                Console.WriteLine($"nic nedosatne");
                                AceFactor = false;
                            }
                            else
                            {
                                PackofCards.DrawCard(player, NumberOfCards);
                            }
                            SevenCount = 0;

                            break;
                        }

                        //SevenAce = CheckSevenAce(lastcard);
                        
                        NewTry = CheckGameLogick(lastcard, player, response); //kontrola, jestli to jde ztahrát a přpadné počítání sedmiček

                        if (QueenFactor == true)
                        {
                            string Message3 = "jakou barvu chceš:" +
                                "\n1) srdce" +
                                "\n2) koule" +
                                "\n3) listy" +
                                "\n4) žaludy";
                            byte[] sendData3 = Encoding.UTF8.GetBytes(Message3);
                            
                            //Thread.Sleep(2000);

                            udpClient.Send(sendData3, sendData3.Length, PlayerOnTurn);
                            Console.WriteLine("odeslana otazka na barvu");

                            //Thread.Sleep(2000);

                            byte[] receivedColor = udpClient.Receive(ref responseEndPoint);
                            int responseColor = int.Parse(Encoding.UTF8.GetString(receivedColor));
                            Console.WriteLine($"Přijatá odpověď ohledně barvy: {responseColor}");

                            Color = GetCardColor(responseColor);
                            Console.WriteLine($"barva: {Color}");

                            QueenFactor = false;
                            //NewTry = false; //pak to presun na logiuctejsi misto ( do chckgameligick

                        }
                        
                        Console.WriteLine($"ten debilni newtry: {NewTry}");
                        if (NewTry == true) 
                        {
                            
                            string Message2 = "jsi blbej, zkus to znova";//uprava odpovědi
                            byte[] sendData2 = Encoding.UTF8.GetBytes(Message2);
                            udpClient.Send(sendData2, sendData2.Length, PlayerOnTurn);
                            Console.WriteLine("ödeslano");
                            //Thread.Sleep(2000);
                        }
                        else //pokud neni pořeba ho nutit znova odpovídat, tak se to uzavře
                        {
                           break; 
                        }
                        
                        
                        
                    } while (true);

                    Console.WriteLine("zvládl to");

                    udpClient.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Chyba: {ex.Message}");
                }
                
                
                
            }


            

        }
        public static bool CheckGameLogick(Card CardonDeck, Player player, int CardonHandIndex)
        {
            CardonHandIndex--;
            Card playedCard = player.PlayersCards[CardonHandIndex];
            
            //na vrchu 7 

            if (CardonDeck.CardValue == "7" && SevenCount > 0)  //jakoze první sedma neplatí
            {
                Console.WriteLine("poslední byla 7");
                if (playedCard.CardValue == "7")
                {
                    SevenCount++;
                    Console.WriteLine($"zahraná sedmička!!!!!!!!!!!!!!{SevenCount}");
                    PackofCards.PlayCard(CardonHandIndex, player);
                    return false;
                }
                else
                {

                    return true;
                }
            }
            
            // na vrchu eso

            else if (CardonDeck.CardValue == "Ace" && AceFactor == true) //jakoze první eso neplatí
            {
                Console.WriteLine("poslední bylo eso");
                if (playedCard.CardValue == "Ace")
                {
                   
                    Console.WriteLine($"zahrany eso");
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
                //Console.WriteLine($"pro jistotu: {playedCard}");
                if (playedCard.CardValue == "7") //sedmička při rozdání se nepočítá
                {
                    SevenCount++;
                    Console.WriteLine($"zahraná sedmička!!!!!!!!!!!!!!{SevenCount}");
                }
                else { SevenCount = 0; }
                if (playedCard.CardValue == "Ace") //sedmička při rozdání se nepočítá
                {
                    AceFactor = true;
                    Console.WriteLine($"eso!!!!!!!!!!!!!!");
                }
                else { AceFactor = false; } //myslim, že jhe to navíc
                if (playedCard.CardValue == "svršek") //sedmička při rozdání se nepočítá
                {
                    QueenFactor = true;
                    Console.WriteLine($"svršek bejby!!!!!!!!!!!!!!");
                }
                else { //Color = null;
                       } //myslim, že jhe to navíc

                Console.WriteLine($"bRV je {Color} hraníá barva je {playedCard.CardColor}");

                
                if (Color != null)  //když hraju na svrška, tak nesmim jeho barvu, ale jenom tu zvolenou
                {
                    if (playedCard.CardColor == Color) 
                    {
                        Console.WriteLine($"karta je zahraná");

                        PackofCards.PlayCard(CardonHandIndex, player);
                        Color = null;
                        return false;
                    }
                    else
                    {
                        Console.WriteLine($"jtypek to nedal");
                        return true;
                    }
                }
                else //pokud nebyl svršek, tak to jde normálně
                {
                    if (playedCard.CardColor == CardonDeck.CardColor || playedCard.CardValue == CardonDeck.CardValue || playedCard.CardValue == "svršek")
                    {

                        Console.WriteLine($"karta je zahraná");

                        PackofCards.PlayCard(CardonHandIndex, player);
                        return false;
                    }
                    else
                    {
                        Console.WriteLine($"jtypek to nedal");
                        return true;
                    }
                }
            }
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
        static bool IsThereWinner(Player player) // zkontoŕoluj
        {
            if(player.PlayersCards.Count == 0)
            {
                Console.WriteLine($"{player} tenhle typek to vyhral jupi");
                //odeber ho z toho listu a asi to nemusi byt bool 
                return true;
            }
            return false;
        }
        /*public static void DealCards() 
        {
            PlayerList.PrintPlayerList();
            PackofCards.PrintDecks();


            int round = 0;
            int WhichPlayer;
            int PlayerCount = PlayerList.playerIPList.Count;
            Console.WriteLine(PlayerCount);
            
            while (round < 4) // 4 kola
            {
                for (WhichPlayer = 0;WhichPlayer < PlayerCount; WhichPlayer++) //inicializátor, podmínka, iterátor = před, podmínkia po - https://learn.microsoft.com/cs-cz/dotnet/csharp/language-reference/statements/iteration-statements#code-try-4
                {
                    PackofCards.DrawCard(WhichPlayer); // Rozdá hráči kartu (pořadí hráčů se bere podle pořadí v listu
                }
                round++;
            }


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
            PackofCards.DrawCard(number35); * /
        }*/


        //public static void CheckForSpecialCard(Card CardonDeck, Player player, int CardonHandIndex)
        //{
        //   CardonHandIndex--;
        //   Card playedCard = player.PlayersCards[CardonHandIndex];

        // }
        //public static bool CheckSevenAce(Card Lastcard, Card PlayerCard)
        //{
        //    if (Lastcard.CardValue == "7")
        //    {
        //
        //    }
        //    
        //    return false;
        //}
    }
}
