using System;
using System.Collections.Generic;
using System.Linq;
using CardsMvc.Models;


namespace CardsMvc.Logic
{
    public class Hand : IComparable<Hand>
    {
        public List<Card> Cards {get;set;}
        public PokerHandRank Rank {get;set;}
        public List<int> Strength {get;set;}
        public int CompareTo(Hand other)
        {
            if(other == null) return 1;

            if(this.Rank != other.Rank)
                return this.Rank.CompareTo(other.Rank);

            for (int i = 0; i < this.Strength.Count; i++)
            {
                if(this.Strength[i] != other.Strength[i])
                    return this.Strength[i].CompareTo(other.Strength[i]);
            }

            return 0;
        }
    }
}