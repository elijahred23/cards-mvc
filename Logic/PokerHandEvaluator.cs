using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;

namespace CardsMvc.Logic
{
    public enum PokerHandRank
    {
        HighCard,
        OnePair,
        TwoPair,
        ThreeOfAKind,
        Straight,
        Flush,
        FullHouse,
        FourOfAKind,
        StraightFlush,
        RoyalFlush
    }
    public static class PokerHandEvaluator
    {
        public static PokerHandRank DetermineRank(IEnumerable<dynamic> hand)
        {
            var cards = hand.ToList();

            if(cards.Count != 5)
                throw new ArgumentException("Hand must contain exactly 5 cards.");


            var sortedRanks = cards.Select(c => (int)c.Rank).OrderByDescending(r => r).ToList();
            var distinctSuits = cards.Select(c => c.Suit).Distinct().Count();


            bool isFlush = distinctSuits == 1;
            bool isStraight = IsStraight(sortedRanks); 

            if(isFlush && isStraight)
            {
                return sortedRanks[0] == 14 ? PokerHandRank.RoyalFlush : PokerHandRank.StraightFlush;
            }

            var rankGroups = cards.GroupBy(c => c.Rank).Select(g =>  new {Count = g.Count(), Rank = (int) g.Key})
            .OrderByDescending(g => g.Count) 
            .ThenByDescending(g => g.Rank)
            .ToList();


            if(rankGroups[0].Count == 4) return PokerHandRank.FourOfAKind;

            if(rankGroups[0].Count == 3 && rankGroups[1].Count == 2) return PokerHandRank.FullHouse;

            if(isFlush) return PokerHandRank.Flush;

            if(isStraight) return PokerHandRank.Straight;

            if(rankGroups[0].Count ==3 ) return PokerHandRank.ThreeOfAKind;

            if(rankGroups[0].Count == 2 && rankGroups[1].Count == 2) return PokerHandRank.TwoPair;

            if(rankGroups[0].Count == 2) return PokerHandRank.OnePair;

            return PokerHandRank.HighCard;
        }

        private static bool IsStraight(List<int> sortedRanks)
        {
            bool standard = true;

            for(int i = 0; i < sortedRanks.Count - 1; i++)
            {
                if(sortedRanks[i] - sortedRanks[i + 1] != 1)
                {
                    standard = false;
                    break;
                }
            }
            if(standard)return true;
            return sortedRanks.SequenceEqual(new List<int> {14, 5, 4, 3, 2});
        }

    }
    
}