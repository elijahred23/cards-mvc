namespace CardsMvc.Models
{
    public enum Suit {Hearts, Diamonds, Clubs, Spades}
    public enum Rank {Two = 2, Three, Four, Five, Size, Seven, Eight, Nine,Ten,Jack, Queen, King, Ace}

    public class Card
    {
        public Suit Suit {get;set;}
        public Rank Rank {get;set;}
        public string DisplayName => $"{Rank} of {Suit}";
    }
}