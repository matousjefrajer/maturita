namespace matura
{
    public class Card
    {
        public string cardColor { get; set; }
        public string cardValue { get; set; }

        public Card(string cardcolor, string cardvalue)
        {
            cardColor = cardcolor;
            cardValue = cardvalue;
        }

        public override string ToString()
        {
            return $"{cardColor} {cardValue}";
        }
    }
}
