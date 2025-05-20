using System.Diagnostics;

namespace matura
{
    internal class GlobalSetting
    {
        public static bool serverAndPlayerOnOneDevice = false; //server and player on one device
        public static int serverPort = 13000;
        public static int returnPort = 13001;
        public static int ID = 0;
        public static bool endOfServer = false;

        private static readonly object lockObj = new object();
        public static void EnterNick()
        {
            lock (lockObj)
            {
                do
                {
                    Player_Client.nick = Console.ReadLine() ?? "";

                    if (string.IsNullOrWhiteSpace(Player_Client.nick))
                    {
                        Console.WriteLine("Přezdívka nesmí být prázdná.");
                    }
                    else if (Player_Client.nick.Length > 20)
                    {
                        Console.WriteLine("Přezdívka je příliš dlouhá. Maximální délka je 20 znaků.");
                    }
                    else if (Player_Client.nick == "z" || Player_Client.nick == "Z")
                    {
                        RestartGame();
                    }

                } while (string.IsNullOrWhiteSpace(Player_Client.nick) || Player_Client.nick.Length > 20);
            }
        }
        public static void RestartGame()
        {
            Console.WriteLine("Chtěl bys dát novou hru, nebo jít do menu? Pokud ano stiskni cokoli. Pokud ne, stiskni esc.");
            var key = Console.ReadKey(true);
            if (key.Key != ConsoleKey.Escape)
            {
                var fileName = Process.GetCurrentProcess().MainModule?.FileName;
                if (!string.IsNullOrEmpty(fileName))
                {
                    Process.Start(fileName);
                    Environment.Exit(0);
                }
                else
                {
                    Console.WriteLine("Omlouvám se, ale nelze restartovat aplikaci.");
                    Environment.Exit(0);
                }
            }
            else
            {
                Environment.Exit(0);
            }
        }
    }
   
}
