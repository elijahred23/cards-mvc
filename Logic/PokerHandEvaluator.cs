using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using CardsMvc.Models;

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

            if(rankGroups[0].Count == 3) return PokerHandRank.ThreeOfAKind;

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
    public static Hand GetBestHand(IEnumerable<Card> sevenCards)
        {
            var cards = sevenCards.ToList();

            if(cards.Count != 7)
            {
                throw new ArgumentException("Seven cards are required for Texas Hold'em evaluation");
            }

            Hand bestHand = null;

            for(int i = 0; i < cards.Count; i++)
            {
                for (int j = i + 1; j < cards.Count; j++)
                {
                    var fiveCardSubset = new List<Card>();

                    for(int k = 0; k < cards.Count; k++)
                    {
                        if(k!=i && k != j)
                        {
                            fiveCardSubset.Add(cards[k]);
                        }
                    }
                    var currentHand = Evaluate(fiveCardSubset);

                    if(bestHand == null || currentHand.CompareTo(bestHand) > 0)
                    {
                        bestHand = currentHand;
                    }
                }
            }

            return bestHand;
        }

        public static Hand Evaluate(IEnumerable<Card> handCards)
        {
            var cards = handCards.ToList();

            var sortedRanks = cards.Select( c => 
            (int)c.Rank).OrderByDescending(r => r).ToList();

            var distinctSuits = cards.Select(c => c.Suit).Distinct().Count();


            bool isFlush = distinctSuits == 1;

            bool isStraight = IsStraight(sortedRanks);

            var groups = cards.GroupBy(c => c.Rank)
            .Select(g => new {Rank =(int)g.Key, Count = g.Count()})
            .OrderByDescending(g => g.Count)
            .ThenByDescending(g=>g.Count)
            .ToList();

            PokerHandRank rank;

            List<int> strength = groups.Select(g => g.Rank).ToList();

            if(isFlush && isStraight)
            {
                bool isAceLow = sortedRanks.SequenceEqual(new List<int> {14, 5, 4, 3, 2});

                rank = (sortedRanks[0] == 14 && !isAceLow) ? PokerHandRank.RoyalFlush : PokerHandRank.StraightFlush;

                strength = new List<int> { isAceLow ? 5 : sortedRanks[0]};
            }
            else if (groups[0].Count == 4) rank = PokerHandRank.FourOfAKind;
            else if (groups[0].Count == 3 && groups[1].Count == 2) rank = PokerHandRank.FullHouse;
            else if (isFlush){ rank = PokerHandRank.Flush; strength = sortedRanks;}
            else if (isStraight)
            {
                rank = PokerHandRank.Straight;
                bool isAceLow = sortedRanks.SequenceEqual(new List<int> {14, 5, 4, 3, 2});
                strength = new List<int> {isAceLow ? 5 : sortedRanks[0]};
            }
            else if (groups[0].Count == 3) rank = PokerHandRank.ThreeOfAKind;
            else if (groups[0].Count == 2 && groups[1].Count == 2) rank = PokerHandRank.TwoPair; 
            else if(groups[0].Count == 2) rank = PokerHandRank.OnePair; 
            else {rank = PokerHandRank.HighCard; strength = sortedRanks;}

            return new Hand {Cards=cards, Rank = rank, Strength=strength};
        }
    }
}