namespace matura
{
    public class Card
    {
        public string CardColor { get; set; }
        public string CardValue { get; set; }

        public Card(string cardcolor, string cardvalue)
        {
            CardColor = cardcolor;
            CardValue = cardvalue;
        }

        public override string ToString()
        {
            return $"{CardColor} {CardValue}";
        }
    }
}
