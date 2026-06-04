using System;
using System.Collections.Generic;
using System.Linq;


namespace CardsMvc.Models
{
    public class Deck
    {
        public List<Card> Cards {get;set;} = new();

        public Deck() => Initialize();
        public void Initialize()
        {
            Cards.Clear();

            foreach (Suit s in Enum.GetValues(typeof(Suit)))
            {
                foreach(Rank r in Enum.GetValues(typeof(Rank)))
                {
                    Cards.Add(new Card {Suit = s, Rank = r});
                }
            }
        }
        public void Shuffle()
        {
            var rnd = new Random();

            Cards = Cards.OrderBy(c => rnd.Next()).ToList();
        }
        public Card Deal()
        {
            if(!Cards.Any()) return null;

            var card = Cards[0];

            Cards.RemoveAt(0);
            return card;
        }
    }
}