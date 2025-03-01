using matura;
using System.Numerics;
//najdi si kde vsude je to od chatu
PackofCards pack = new PackofCards(); //vytvoření herního balíčku

server.Search();

PackofCards.DealCards();

string message = "///////ZAČALA HRA\\\\\\\\\\\\\\";
Game.SendToAll(message);

Console.Clear();
Console.WriteLine("Můžeš pozorovat hru, nebo stiski \"S\" poro znovuodeslání zprávy, nebo \"R\" pro vyhození hráče");
while (Game.MoreThenOnePlayer)
{
    Game.game();

}
Console.WriteLine($"Konec hry");