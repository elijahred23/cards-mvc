# Poker Hand Determination Logic

This reference implementation shows how to determine the rank of a 5-card poker hand using C#.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

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
            if (cards.Count != 5)
                throw new ArgumentException("Hand must contain exactly 5 cards.");

            // Assumes Card has properties Rank (enum/int) and Suit (enum/int)
            // Ace is assumed to be 14 for evaluation
            var sortedRanks = cards.Select(c => (int)c.Rank).OrderByDescending(r => r).ToList();
            var distinctSuits = cards.Select(c => c.Suit).Distinct().Count();

            bool isFlush = distinctSuits == 1;
            bool isStraight = IsStraight(sortedRanks);

            if (isFlush && isStraight)
            {
                return sortedRanks[0] == 14 ? PokerHandRank.RoyalFlush : PokerHandRank.StraightFlush;
            }

            var rankGroups = cards.GroupBy(c => c.Rank)
                                  .Select(g => new { Count = g.Count(), Rank = (int)g.Key })
                                  .OrderByDescending(g => g.Count)
                                  .ThenByDescending(g => g.Rank)
                                  .ToList();

            if (rankGroups[0].Count == 4) return PokerHandRank.FourOfAKind;
            if (rankGroups[0].Count == 3 && rankGroups[1].Count == 2) return PokerHandRank.FullHouse;
            if (isFlush) return PokerHandRank.Flush;
            if (isStraight) return PokerHandRank.Straight;
            if (rankGroups[0].Count == 3) return PokerHandRank.ThreeOfAKind;
            if (rankGroups[0].Count == 2 && rankGroups[1].Count == 2) return PokerHandRank.TwoPair;
            if (rankGroups[0].Count == 2) return PokerHandRank.OnePair;

            return PokerHandRank.HighCard;
        }

        private static bool IsStraight(List<int> sortedRanks)
        {
            bool standard = true;
            for (int i = 0; i < sortedRanks.Count - 1; i++)
            {
                if (sortedRanks[i] - sortedRanks[i + 1] != 1)
                {
                    standard = false;
                    break;
                }
            }
            if (standard) return true;
            return sortedRanks.SequenceEqual(new List<int> { 14, 5, 4, 3, 2 });
        }
    }
}
```

## Web Implementation Guide

To integrate this into your MVC application, you can use the following Controller and View structure.

### 1. PokerController.cs

This controller manages a static deck, shuffles it, deals 5 cards, and calls the evaluator.

```csharp
using Microsoft.AspNetCore.Mvc;
using CardsMvc.Models;
using CardsMvc.Logic;
using System.Collections.Generic;

namespace CardsMvc.Controllers
{
    public class PokerController : Controller
    {
        // Use a static deck to persist state between requests if desired, 
        // or initialize a new one per Deal.
        private static readonly Deck _deck = new();

        public IActionResult Index()
        {
            // Refresh and Shuffle the deck
            _deck.Initialize();
            _deck.Shuffle();
            
            var hand = new List<Card>();
            for (int i = 0; i < 5; i++)
            {
                var card = _deck.Deal();
                if (card != null) hand.Add(card);
            }

            // Determine the rank using the logic provided above
            var rank = PokerHandEvaluator.DetermineRank(hand);
            
            ViewBag.Rank = rank.ToString();
            return View(hand);
        }
    }
}
```

### 2. Index.cshtml (View)

A simple view to display the cards dealt and the resulting poker hand rank.

```html
@model List<CardsMvc.Models.Card>

<div class="text-center">
    <h1 class="display-4">Poker Hand Evaluator</h1>
    <h2 class="text-primary">Result: @ViewBag.Rank</h2>

    <div class="d-flex justify-content-center my-4">
        @foreach (var card in Model)
        {
            <div class="card mx-2 p-3 shadow-sm" style="width: 100px;">
                @card.DisplayName
            </div>
        }
    </div>

    <a asp-action="Index" class="btn btn-success btn-lg">Deal Again</a>
</div>
```