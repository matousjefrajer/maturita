namespace matura
{
    internal class Player_Visuals
    {
        public static string serverPlayer = "";
        public static string whoIsOnTurn = "";
        public static string cardOnTop = "";
        public static string cards = "";
        public static string lastCard = "";

        private static string[] history = new string[4];
        private static string[] names = new string[7];
        private static string[] counts = new string[7];
       
        public static void UpdateScreen()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($" {serverPlayer} " + 
                $"\n    _____________________________" +
                $"\n    |Historie hry:               " +
                $"\n    |{history[0]}                  " +
                $"\n    |{history[1]}                  " +
                $"\n    |{history[2]}                  " +
                $"\n    |{history[3]}                  " +
                $"\n    |____________________________" +
                $"\n    |Hraje:             " +
                $"\n    | {whoIsOnTurn}     " +
                $"\n    |________________________________________________________    " +
                $"\n    |Hráči + počet karet:___________________________________|    " +
                $"\n    |{string.Join("|", names)}|    " +
                $"\n    |{string.Join("|", counts)}|    " +
                $"\n    |_______________________________________________________|    " +
                $"\n ");
            Console.ForegroundColor = ConsoleColor.Cyan; 
            Console.WriteLine($"                       {lastCard}");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"{cards}                                           ");
            Console.ResetColor();
        }
        public static void UpdateHistory(string newhistory)
        {
            Array.Copy(history, 0, history, 1, history.Length - 1); //posouvá všechny starší záznamy o jednu pozici dolů,
            history[0] = newhistory;                                

        }
        public static void UpdatePlayers(string[] players)
        {
            Array.Fill(names, "       ");
            Array.Fill(counts, "       ");

            for (int i = 0; i < players.Length && i < names.Length; i++) //konzultace s chatGPT
            {
                string[] info = players[i].Split(',');

                names[i] = info[0].PadRight(7).Substring(0, 7);
                counts[i] = info[1].PadRight(7).Substring(0, 7);
            }
        }
    }
}
