using matura;
using System.Numerics;
//najdi si kde vsude je to od chatu
PackofCards pack = new PackofCards(); //vytvoření herního balíčku

server.Search();

//PlayerList.AddPlayer("00000000");
//PlayerList.AddPlayer("00000100");
//PlayerList.AddPlayer("00000002");
PackofCards.DealCards();

while (true)
{
    Game.game();

}
