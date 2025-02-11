using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace matura
{
    public class Card
    {
        public string CardColor { get; set; }  //nstavim si ty veci
        public string CardValue { get; set; }

        public Card(string cardcolor, string cardvalue) //konstruktor
        {
            CardColor = cardcolor;
            CardValue = cardvalue;
        }

        public override string ToString() //přeformátování pro výpis, aby to bylo srozumitelny - porad moc nechapu proc to nejde bez toho
        {
            return $"{CardColor} {CardValue}";
        }
    }
}
