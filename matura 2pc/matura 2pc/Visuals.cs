using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace matura_2pc
{
    internal class Visuals
    {
        public static string history1 = "";
        public static string history2 = "";
        public static string history3 = "";
        public static string history4 = "";

        public static string WhoIsOnTurn = "";

        public static string Name1 = "       ";
        public static string Name2 = "       ";
        public static string Name3 = "       ";
        public static string Name4 = "       ";
        public static string Name5 = "       ";
        public static string Name6 = "       ";
        public static string Name7 = "       ";

        public static string Count1 = "       ";
        public static string Count2 = "       ";
        public static string Count3 = "       ";
        public static string Count4 = "       ";
        public static string Count5 = "       ";
        public static string Count6 = "       ";
        public static string Count7 = "       ";
        
        public static string CardOnTop = "";

        public static string Cards = "";
        public static string LastCard = "";

        public static void UpdateScreen()
        {
            Console.Clear();
            Console.WriteLine($"  " + //{Game}
                $"\n    _____________________________" +
                $"\n    |Historie hry:               " +
                $"\n    |{history1}                  " +
                $"\n    |{history2}                  " +
                $"\n    |{history3}                  " +
                $"\n    |{history4}                  " +
                $"\n    |____________________________" +
                $"\n    |Hraje:             " +
                $"\n    | {WhoIsOnTurn}     " +
                $"\n    |________________________________________________________    " +
                $"\n    |Hráči + počet karet:___________________________________|    " +
                $"\n    |{Name1}|{Name2}|{Name3}|{Name4}|{Name5}|{Name6}|{Name7}|    " +
                $"\n    |{Count1}|{Count2}|{Count3}|{Count4}|{Count5}|{Count6}|{Count7}|    " +
                $"\n    |_______________________________________________________|    " +
                $"\n ");
            Console.ForegroundColor = ConsoleColor.Cyan; //chatgpt
            Console.WriteLine($"                       {LastCard}");
            Console.ResetColor();
            Console.WriteLine($"{Cards}                                           ") ;
        }
        public static void UpdateHistory(string newhistory)
        {
            history4 = history3;
            history3 = history2;
            history2 = history1;
            history1 = newhistory;
        }
        public static void UpdatePlayers(string[] players)
        {
            Visuals.Name1 = Visuals.Name2 = Visuals.Name3 = Visuals.Name4 = Visuals.Name5 = Visuals.Name6 = Visuals.Name7 = "       "; //vyčistim to, aby tam nezůstavali výherci
            Visuals.Count1 = Visuals.Count2 = Visuals.Count3 = Visuals.Count4 = Visuals.Count5 = Visuals.Count6 = Visuals.Count7 = "       "; 

            for (int i = 0; i < players.Length; i++)
            {
                string[] playerinfo = players[i].Split(','); // rozdělíme na jméno a počet karet

                string playerName = playerinfo[0].Length > 7 ? playerinfo[0].Substring(0, 7) : playerinfo[0].PadRight(7); //chat gpt - jak dělat, aby měli vždy 7 znaků, aby se to nerozházelo
                string cardCount = playerinfo[1].Length > 7 ? playerinfo[1].Substring(0, 7) : playerinfo[1].PadRight(7); //chat gpt - jak dělat, aby měli vždy 7 znaků, aby se to nerozházelo


                if (i == 0)
                {
                    Visuals.Name1 = playerName;
                    Visuals.Count1 = cardCount;
                }
                else if (i == 1)
                {
                    Visuals.Name2 = playerName;
                    Visuals.Count2 = cardCount;
                }
                else if (i == 2)
                {
                    Visuals.Name3 = playerName;
                    Visuals.Count3 = cardCount;
                }
                else if (i == 3)
                {
                    Visuals.Name4 = playerName;
                    Visuals.Count4 = cardCount;
                }
                else if (i == 4)
                {
                    Visuals.Name5 = playerName;
                    Visuals.Count5 = cardCount;
                }
                else if (i == 5)
                {
                    Visuals.Name6 = playerName;
                    Visuals.Count6 = cardCount;
                }
                else if (i == 6)
                {
                    Visuals.Name7 = playerName;
                    Visuals.Count7 = cardCount;
                }
            }

        }
    }
}
