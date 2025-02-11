using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace matura
{
    internal class Player
    {
        public string IPAddress;
        //public int PlayerNumber;
        public List<Card> PlayersCards;
        public Player(string ipaddress, List<Card> playercards) //konstruktor int playernumber,
        {
            IPAddress = ipaddress;
            //PlayerNumber = playernumber;
            PlayersCards = playercards;
        }
        public override string ToString() //přeformátování pro výpis, aby to bylo srozumitelny - porad moc nechapu proc to nejde bez toho
        {
            return $"{IPAddress}  {PlayersCards}";//{PlayerNumber}
        }
    }
}
