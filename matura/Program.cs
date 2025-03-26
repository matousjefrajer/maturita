using matura;
using System.Net.Sockets;
using System.Net;
using System.Text;

Welcome();

while (true)
{
    switch (Console.ReadKey(intercept: true).Key)
    {
        case ConsoleKey.P:

            Rules();
            break;

        case ConsoleKey.J:

            HowToPlay();
            break;

        case ConsoleKey.I:

            ChangeGameID();
            break;

        case ConsoleKey.S:
    
            GlobalSetting.SaPOnOneDevice = false;
            Serverpart();
            return;

         case ConsoleKey.H:
    
            GlobalSetting.SaPOnOneDevice = false;
            GlobalSetting.EndOfServer = true;
            PlayerPart();
            return;

        case ConsoleKey.O:
    
            GlobalSetting.SaPOnOneDevice = true;
            Task serverTask = Task.Run(Serverpart);
            Task playerTask = Task.Run(PlayerPart);
            Task.WaitAll(serverTask, playerTask);
            return;

        case ConsoleKey.Z:
    
            GlobalSetting.SaPOnOneDevice = false;
            Reconnect();
            return;

        default:
    
            Console.WriteLine($"zkus to znova");
            break;
    }
}

void Welcome()
{
    Console.Clear();
    
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("╔═════════════════════════════════════════════════════════════════════════════════╗" +
                    "\n║                                Vítej ve hře prší                                ║" +
                    "\n╠═════════════════════════════════════════════════════════════════════════════════╣");
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("║ - aplikace umožňuje online hru prší přes síť                                    ║" +
                    "\n║ - jak založit hru? - dozvíš se po stisknutí: \"J\"                                ║" +
                    "\n║ - pro hru je potřeba mít zařízení připojená na stejné wifi, či mobilním hotspotu║" +
                    "\n║ - pokud nemáte dostatek spoluhráčů, tak existuje možnost přidat do hry \"Boty\"   ║" +
                    "\n║ - boti jsou hráči řízeni počítačem a dají se do hry přidat při vyhledávání hráčů║" +
                    "\n║ - pokud jsi sám, tak stejně můžeš zvolit: \"B\" a přidat si do hry pár botů       ║" +
                    "\n║   a hrát pouze proti počítači.                                                  ║");
    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.WriteLine("║ - tato hra se ovládá mačkáním určitých tlačítek na klávesnici, či zápisem       ║" +
                    "\n║   textu a potvrzením entrem                                                     ║");
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("║ - pokud si chceš přečíst pravidla hry, tak stiskni \"P\"                          ║");
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("╠═════════════════════════════════════════════════════════════════════════════════╣" +
                    "\n║ Pokud jsi připraven na hru, tak stiskni:                                        ║");
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("║ \"S\" - Server = chceš založit server                                             ║" +
                    "\n║ \"H\" - Hráč = někdo jiný již server založil                                      ║" +
                    "\n║ \"O\" - Server i hráč = chceš založit server i hrát                               ║" + 
                    "\n║ \"Z\" - Znovu se připojit = připojit se do již běžící hry po tom, co ti to spadlo ║" +
                   $"\n║ \"I\" - Změna ID - teď máš {GlobalSetting.ID} - musí se shodovat s ID serveru                      ║");
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("╚═════════════════════════════════════════════════════════════════════════════════╝");
    Console.ResetColor(); //chatGPT mi řekl, že existujou ty znaky na ohraničení
}
void Serverpart()
{
    PackofCards pack = new PackofCards(); //vytvoření herního balíčku + míchání
    
    Server_Server.Search();
    
    PackofCards.DealCards();
    
    string message = "///////ZAČALA HRA\\\\\\\\\\\\\\";
    Server_Game.SendToAll(message);
    
    Console.Clear();
    Console.WriteLine("Můžeš pozorovat hru.");

    if (GlobalSetting.SaPOnOneDevice == true) Player_Visuals.ServerPlayer = "stisknutí: \"V\" = můžeš někoho vyhodit, \"O\" = opuštění hry, \"K\" = konec hry";

    while (Server_Game.MoreThenOnePlayer)
    {
        Server_Game.game();
    }
    Console.WriteLine($"Konec hry");
}
void PlayerPart()
{
    if (GlobalSetting.SaPOnOneDevice == true)
    {
        Console.Clear();
        Console.WriteLine("Server již běží a hledá hráče. Zadej svoji přezdívku a také se připoj");
        Player_Visuals.ServerPlayer = "stisknutí: \"B\" = přidání bota, \"K\" = ukončení hledání hráčů a začátek hry, \"V\" = vyhození hráče";
    }
    
    Console.WriteLine("Pokud se chceš vrátit, zadej \"Z\"");
    Console.WriteLine("Zadej svoji přezdívku:");
    GlobalSetting.EnterNick();
    Player_Client.Search();
    Player_Game.Comunication();    
}
void Reconnect()
{
    Console.WriteLine("Pokud se chceš vrátit, zadej \"Z\"");
    Console.WriteLine("Zadej svůji původní přezdívku:");
    GlobalSetting.EnterNick(); 
    Console.WriteLine("Hledání serveru... (může to chvíli trvat)");

    UdpClient udpClient = new UdpClient();

    IPEndPoint IPEndPoint = new IPEndPoint(IPAddress.Broadcast, GlobalSetting.ReturnPort);

    udpClient.Client.ReceiveTimeout = 3000;
    bool StillSend = true;
    while (StillSend)
    {
        try
        {
            Console.WriteLine("Hledání serveru...");

            string Message = $"LOOKINGFORSERVER.{Player_Client.Nick}";
            byte[] MessegeData = Encoding.UTF8.GetBytes(Message);
            udpClient.Send(MessegeData, MessegeData.Length, IPEndPoint);

            Player_Client.ClientPort = (udpClient.Client.LocalEndPoint as IPEndPoint)?.Port ?? 0;

            Player_Client.ServerEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] serverResponse = udpClient.Receive(ref Player_Client.ServerEndPoint);
            string responseMessage = Encoding.UTF8.GetString(serverResponse);

            switch (responseMessage)
            {
                case "MAUMAUSERVER":
                    Console.WriteLine($"Server found at IP: {Player_Client.ServerEndPoint}");
                    StillSend = false;
                    break;
                case "WRONGNAME":
                    Console.WriteLine("Tvůj nick nebyl nalezen, nebo je někoho jiného. Zkus to znova:");
                    GlobalSetting.EnterNick();
                    break;
            }

        }
        catch (SocketException e) when (e.SocketErrorCode != SocketError.TimedOut) 
        {
            Console.WriteLine($"Chyba: {e} ");
        }
    }

    udpClient.Close();

    Console.WriteLine("Našel jsi zase svou hru. Tvé karty se ti načtou, když budeš na tahu a historie se zase vyplní");
    Player_Game.Comunication();
}
void Rules()
{
    Console.WriteLine("----------------------------------------------------------------------------------------------------" +
                    "\nPravidla:" +
                    "\n----------------------------------------------------------------------------------------------------" +
                    "\n- Hraje se s 32 kartami (hodnoty 7–eso, barvy: listy, žaludy, srdce, kule)." +
                    "\n- Na stole jsou 2 balíčky: lízací (z něj se berou karty) a odhazovací (sem se hrají karty)." +
                    "\n- Pokud dojde lízací balíček, zamíchá se odhazovací balíček (kromě vrchní karty) a stane se novým lízacím." +
                    "\n" +
                    "\nHra:" +
                    "\n- Každý hráč dostane 4 karty." +
                    "\n- Z lízacího balíčku se vyloží první karta, která začne odhazovací balíček (pokud je to speciální karta, " +
                    "\n  její vlastnost neplatí)." +
                    "\n- Hráči musí hrát kartu stejné barvy nebo hodnoty jako vrchní karta v odhazovacím balíčku." +
                    "\n- Když se ti na konci tabulky hráče objevý červený text, tak jsi na tahu." +
                    "\n" +
                    "\nSpeciální karty:" +
                    "\n- Eso: Následující hráč musí zahrát eso nebo \"stát\" (přeskočit tah a nic nezahrát)." +
                    "\n- Svršek: Mění barvu (lze zahrát na cokoli kromě 7 nebo esa, které platí)." +
                    "\n  Příklad: Žaludový svršek zahraný na listy může měnit barvu na srdce. Další hráč by musel hrát srdce." +
                    "\n- Sedma: Následující hráč bere 2 karty, nebo může hrát další sedmu (pak další bere 4, pak 6, atd.)." +
                    "\n" +
                    "\nKonec hry:" +
                    "\n- Vyhrává hráč, který první odehraje všechny karty, poté se hra dohrává o další místa." +
                    "\n- Při předčasném konci se hráči řadí: ti, co dohráli, jsou první, ostatní podle počtu karet v ruce." +
                    "\n----------------------------------------------------------------------------------------------------"); 
}
void HowToPlay()
{
    Console.WriteLine(
                    "----------------------------------------------------------------------------------------------------" +
                    "\nJak založit hru:  " +
                    "\n---------------------------------------------------------------------------------------------------- " +
                    "\n Jeden hráč musí založit server:" +
                    "\n - Zvolte \"S\" (pouze server) nebo \"B\" (server + hráč)." +
                    "\n - Ostatní hráči se připojí jako klienti (\"P\").  " +
                    "\n - Vetšinou je potřeba vypnout antivir, aby se hra dala hrát.  " +
                    "\n" +
                    "\n Pro více her v jedné síti:    " +
                    "\n - Nastavte jiné ID hry (\"I\") - v základu 0." +
                    "\n - Všichni hráči i server musí mít stejné ID, aby se našli. " +
                    "\n - Při pokusu o vytvoření serveru hra zkontroluje dostupnost ID:" +
                    "\n   - Pokud je ID volné, server se spustí" +
                    "\n   - Pokud je obsazené, hra vás vyzve k zadání jiného ID, dokud nenajdete volné." +
                    "\n----------------------------------------------------------------------------------------------------");
}
void ChangeGameID()
{
    Console.WriteLine("Napiš nové ID (doporučuju 0 - 10):");
    while (true) 
    {
        int id;
        string input = Console.ReadLine() ?? "";

        if (int.TryParse(input, out id) && id >= 0)
        {
            GlobalSetting.ServerPort = 13000 + id * 2;
            GlobalSetting.ReturnPort = 13001 + id * 2;
            Console.WriteLine("ID bylo změněno.");
            break; 
        }

        Console.WriteLine("Zkus to znova.");
    }
}
