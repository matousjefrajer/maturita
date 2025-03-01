using matura_2pc;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

//Visuals.UpdateScreen();

do
{
    Console.WriteLine("Zadej svůj nick, nebo napiš \"P\", pokud ses již připojil a spadlo ti to.");
    Client.Nick = Console.ReadLine() ?? "";

    if (string.IsNullOrWhiteSpace(Client.Nick))
    {
        Console.WriteLine("Nick nesmí být prázdný");
    }
    else if (Client.Nick.Length > 20)
    {
        Console.WriteLine("Nick je příliš dlouhý. Maximální délka je 20 znaků.");
    }

} while (string.IsNullOrWhiteSpace(Client.Nick) || Client.Nick.Length > 20);

if (Client.Nick == "P" || Client.Nick == "p")
{
    Console.WriteLine("Hledání serveru (pokud jsi měl být na tahu, tak požádej někoho u serveru o zaslání zprávy)");
    using (UdpClient udpClient = new UdpClient(Client.Port))
    {
        IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Any, Client.Port); //tady je to sus, uz muzes psat na konkretni ip
        byte[] serverResponse = udpClient.Receive(ref serverEndPoint);

        Client.ServerIP = serverEndPoint.Address.ToString();
        Console.WriteLine("našel jsi zase svou hru, pokud bys měl být na tahu, požádej nekoho u serveru, ať ti pošle znovu zprávu");

    }
    Game.Comunication();
}
else
{
    Client.Search();
    Game.Comunication();
}