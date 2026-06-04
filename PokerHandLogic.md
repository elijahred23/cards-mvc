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

## Feature Implementation: Visual Hand Comparison

This implementation allows the application to deal two hands (Player vs. House) and determine the winner using standard poker tie-breaking rules (kickers).

### 1. The Hand Model

To compare two hands, we need to track not just the `Rank` (e.g., One Pair), but also the specific card values used to break ties (the `Strength`).

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using CardsMvc.Models;

namespace CardsMvc.Logic
{
    public class Hand : IComparable<Hand>
    {
        public List<Card> Cards { get; set; }
        public PokerHandRank Rank { get; set; }
        
        // Strength contains card ranks sorted by priority (count, then rank)
        // e.g., Two Pair (JJ, 22, A) would be [11, 2, 14]
        public List<int> Strength { get; set; } 

        public int CompareTo(Hand other)
        {
            if (other == null) return 1;

            // 1. Compare Primary Hand Rank
            if (this.Rank != other.Rank)
                return this.Rank.CompareTo(other.Rank);

            // 2. Compare Strength Sequence (Tie-breakers/Kickers)
            for (int i = 0; i < this.Strength.Count; i++)
            {
                if (this.Strength[i] != other.Strength[i])
                    return this.Strength[i].CompareTo(other.Strength[i]);
            }

            return 0; // Absolute tie (Split pot)
        }
    }
}
```

### 2. Enhanced Evaluator

The evaluator is updated to return the full `Hand` object. The `Strength` property is calculated by grouping cards by rank and sorting them by frequency, then by rank value.

```csharp
public static class PokerHandEvaluator
{
    public static Hand Evaluate(IEnumerable<Card> handCards)
    {
        var cards = handCards.ToList();
        var sortedRanks = cards.Select(c => (int)c.Rank).OrderByDescending(r => r).ToList();
        var distinctSuits = cards.Select(c => c.Suit).Distinct().Count();

        bool isFlush = distinctSuits == 1;
        bool isStraight = IsStraight(sortedRanks);
        
        var groups = cards.GroupBy(c => c.Rank)
            .Select(g => new { Rank = (int)g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .ThenByDescending(g => g.Rank)
            .ToList();

        PokerHandRank rank;
        List<int> strength = groups.Select(g => g.Rank).ToList();

        if (isFlush && isStraight)
        {
            bool isAceLow = sortedRanks.SequenceEqual(new List<int> { 14, 5, 4, 3, 2 });
            rank = (sortedRanks[0] == 14 && !isAceLow) ? PokerHandRank.RoyalFlush : PokerHandRank.StraightFlush;
            strength = new List<int> { isAceLow ? 5 : sortedRanks[0] };
        }
        else if (groups[0].Count == 4) rank = PokerHandRank.FourOfAKind;
        else if (groups[0].Count == 3 && groups[1].Count == 2) rank = PokerHandRank.FullHouse;
        else if (isFlush) { rank = PokerHandRank.Flush; strength = sortedRanks; }
        else if (isStraight)
        {
            rank = PokerHandRank.Straight;
            bool isAceLow = sortedRanks.SequenceEqual(new List<int> { 14, 5, 4, 3, 2 });
            strength = new List<int> { isAceLow ? 5 : sortedRanks[0] };
        }
        else if (groups[0].Count == 3) rank = PokerHandRank.ThreeOfAKind;
        else if (groups[0].Count == 2 && groups[1].Count == 2) rank = PokerHandRank.TwoPair;
        else if (groups[0].Count == 2) rank = PokerHandRank.OnePair;
        else { rank = PokerHandRank.HighCard; strength = sortedRanks; }

        return new Hand { Cards = cards, Rank = rank, Strength = strength };
    }
    
    // ... IsStraight implementation ...
}
```

### 3. Comparison Controller Action

```csharp
public IActionResult Compare()
{
    _deck.Initialize();
    _deck.Shuffle();

    var playerHand = PokerHandEvaluator.Evaluate(Enumerable.Range(0, 5).Select(_ => _deck.Deal()));
    var houseHand = PokerHandEvaluator.Evaluate(Enumerable.Range(0, 5).Select(_ => _deck.Deal()));

    int comparison = playerHand.CompareTo(houseHand);
    
    ViewBag.Message = comparison > 0 ? "Player Wins!" : (comparison < 0 ? "House Wins!" : "Split Pot!");
    
    return View((Player: playerHand, House: houseHand));
}
```

### 4. Comparison View (`Compare.cshtml`)

```html
@model (CardsMvc.Logic.Hand Player, CardsMvc.Logic.Hand House)

<div class="text-center mt-5">
    <h1 class="display-2">@ViewBag.Message</h1>
    
    <div class="row mt-5">
        <div class="col-md-6 border-end">
            <h2 class="text-primary">Player: @Model.Player.Rank</h2>
            <div class="d-flex justify-content-center">
                @foreach(var card in Model.Player.Cards) { <div class="card p-2 m-1 shadow-sm">@card.DisplayName</div> }
            </div>
        </div>
        <div class="col-md-6">
            <h2 class="text-danger">House: @Model.House.Rank</h2>
            <div class="d-flex justify-content-center">
                @foreach(var card in Model.House.Cards) { <div class="card p-2 m-1 shadow-sm">@card.DisplayName</div> }
            </div>
        </div>
    </div>
    <a asp-action="Compare" class="btn btn-success btn-lg mt-5">Next Round</a>
</div>
```