# Texas Hold'em 7-Card Evaluation Logic

In Texas Hold'em, a player's final hand is determined by picking the best 5-card combination out of 7 available cards (2 private hole cards and 5 shared community cards).

## The "7 Choose 5" Problem

Mathematically, there are exactly **21 possible combinations** of 5 cards that can be formed from a set of 7. The logic follows a simple "Brute Force" approach which is standard and efficient for this scale:

1.  **Generate** all 21 possible 5-card subsets.
2.  **Evaluate** each subset using the existing `PokerHandEvaluator.Evaluate` logic.
3.  **Compare** the results to find the hand with the highest rank and strength.

---

## Step-by-Step Walkthrough

### 1. Define the Input
The method should accept an `IEnumerable<Card>` containing exactly 7 cards. 

### 2. Generate Combinations
While you could use a complex recursive algorithm, the simplest way to get 21 combinations is to use nested loops. Instead of picking 5, it is often easier to logic out which 2 cards to **exclude**.

### 3. Iterative Evaluation
Initialize a variable `bestHand` as `null`. As you iterate through each of the 21 combinations:
*   Create a 5-card list.
*   Pass it to `PokerHandEvaluator.Evaluate(combination)`.
*   If `bestHand` is null or the current hand is stronger (`current.CompareTo(bestHand) > 0`), update `bestHand`.

### 4. Return the Result
After all 21 iterations, `bestHand` will contain the optimal 5-card subset and its associated `PokerHandRank`.

---

## Complete Implementation Solution

Below is the complete logic to be integrated into your `PokerHandEvaluator` class. This includes the 7-card combination logic, the 5-card evaluation logic, and the straight detection helper.

```csharp
    public static class PokerHandEvaluator
    {
        /// <summary>
        /// Finds the best 5-card hand out of 7 cards (Texas Hold'em).
        /// </summary>
        public static Hand GetBestHand(IEnumerable<Card> sevenCards)
        {
            var cards = sevenCards.ToList();
            if (cards.Count != 7)
                throw new ArgumentException("Seven cards are required for Texas Hold'em evaluation.");

            Hand bestHand = null;

            // Brute force: Generate all 21 combinations by picking 2 cards to exclude
            for (int i = 0; i < cards.Count; i++)
            {
                for (int j = i + 1; j < cards.Count; j++)
                {
                    var fiveCardSubset = new List<Card>();
                    for (int k = 0; k < cards.Count; k++)
                    {
                        if (k != i && k != j)
                        {
                            fiveCardSubset.Add(cards[k]);
                        }
                    }

                    var currentHand = Evaluate(fiveCardSubset);

                    // Hand implements IComparable<Hand>
                    if (bestHand == null || currentHand.CompareTo(bestHand) > 0)
                    {
                        bestHand = currentHand;
                    }
                }
            }

            return bestHand;
        }

        /// <summary>
        /// Evaluates a 5-card hand and determines its rank and relative strength.
        /// </summary>
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

        private static bool IsStraight(List<int> sortedRanks)
        {
            bool standard = true;
            for (int i = 0; i < sortedRanks.Count - 1; i++)
            {
                if (sortedRanks[i] - sortedRanks[i + 1] != 1) { standard = false; break; }
            }
            return standard || sortedRanks.SequenceEqual(new List<int> { 14, 5, 4, 3, 2 });
        }
    }
```

## Web Controller Implementation (Texas Hold'em)

To move from a basic 5-card draw to Texas Hold'em, update your `PokerController.cs` to handle hole cards and shared community cards.

### Updated Compare Action

This logic simulates a full showdown by dealing 2 cards to each side and 5 to the board, then evaluating the result.

```csharp
    public IActionResult Compare()
    {
        var deck = new Deck();
        deck.Initialize();
        deck.Shuffle();

        // 1. Deal 2 private hole cards to each player
        var playerHole = Enumerable.Range(0, 2).Select(_ => deck.Deal()).ToList();
        var houseHole = Enumerable.Range(0, 2).Select(_ => deck.Deal()).ToList();

        // 2. Deal 5 shared community cards (Flop, Turn, River)
        var communityCards = Enumerable.Range(0, 5).Select(_ => deck.Deal()).ToList();

        // 3. Find the best 5-card hand for each from the 7 available cards
        var playerBest = PokerHandEvaluator.GetBestHand(playerHole.Concat(communityCards));
        var houseBest = PokerHandEvaluator.GetBestHand(houseHole.Concat(communityCards));

        // 4. Compare the two best hands
        int comparison = playerBest.CompareTo(houseBest);

        ViewBag.Message = comparison > 0 ? "Player Wins!" : (comparison < 0 ? "House Wins!" : "Split Pot!");

        // It is recommended to create a ViewModel for this, 
        // but you can pass a Tuple or use ViewBag for the community cards.
        return View((
            Player: playerBest, 
            House: houseBest, 
            Community: communityCards,
            PlayerHole: playerHole,
            HouseHole: houseHole
        ));
    }
```

> **Note**: You will also need to update your `Compare.cshtml` view to display the `Community` cards in the center and the specific `Hole` cards for each player to reflect a real Texas Hold'em layout.



## Web View Implementation (Texas Hold'em)

Finally, update your `Compare.cshtml` view to display the new Texas Hold'em layout. This view receives the tuple from the controller containing the community cards and individual hole cards.

```razor
@model (CardsMvc.Models.Hand Player, CardsMvc.Models.Hand House, List<CardsMvc.Models.Card> Community, List<CardsMvc.Models.Card> PlayerHole, List<CardsMvc.Models.Card> HouseHole)

<div class="text-center mt-4">
    <h1>Texas Hold'em Showdown</h1>
    <h2 class="alert alert-info py-3 my-4 shadow-sm">@ViewBag.Message</h2>

    <div class="community-cards-section mb-5 p-4 bg-light rounded shadow-sm border">
        <h3 class="mb-3 text-muted text-uppercase small">Community Cards (The Board)</h3>
        <div class="d-flex justify-content-center gap-3">
            @foreach (var card in Model.Community)
            {
                <div class="card p-2 border-2 shadow-sm" style="width: 90px; min-height: 130px;">
                    <div class="fw-bold fs-4">@card.Rank</div>
                    <div class="text-muted">@card.Suit</div>
                </div>
            }
        </div>
    </div>

    <div class="row g-5">
        <!-- Player Side -->
        <div class="col-md-6 border-end">
            <div class="p-3 bg-primary bg-opacity-10 rounded shadow-sm border border-primary border-opacity-25">
                <h3 class="text-primary border-bottom border-primary border-opacity-25 pb-2">Player Hand</h3>
                <p class="lead">Best Rank: <strong>@Model.Player.Rank</strong></p>
                
                <h5 class="mt-4 text-muted small text-uppercase">Your Hole Cards</h5>
                <div class="d-flex justify-content-center gap-2 mb-4">
                    @foreach (var card in Model.PlayerHole)
                    {
                        <div class="card p-2 bg-primary text-white border-white shadow" style="width: 80px; min-height: 110px;">
                             <div class="fw-bold fs-5">@card.Rank</div>
                             <div class="small">@card.Suit</div>
                        </div>
                    }
                </div>

                <h5 class="text-muted small text-uppercase">Winning 5-Card Combination</h5>
                <div class="d-flex justify-content-center gap-1">
                    @foreach (var card in Model.Player.Cards)
                    {
                        <div class="card p-1 border-secondary shadow-sm" style="width: 60px; min-height: 90px;">
                             <div class="fw-bold">@card.Rank</div>
                             <div style="font-size: 0.7rem">@card.Suit</div>
                        </div>
                    }
                </div>
            </div>
        </div>

        <!-- House Side -->
        <div class="col-md-6">
            <div class="p-3 bg-dark bg-opacity-10 rounded shadow-sm border border-dark border-opacity-25">
                <h3 class="text-dark border-bottom border-dark border-opacity-25 pb-2">House Hand</h3>
                <p class="lead">Best Rank: <strong>@Model.House.Rank</strong></p>

                <h5 class="mt-4 text-muted small text-uppercase">House Hole Cards</h5>
                <div class="d-flex justify-content-center gap-2 mb-4">
                    @foreach (var card in Model.HouseHole)
                    {
                        <div class="card p-2 bg-dark text-white shadow" style="width: 80px; min-height: 110px;">
                             <div class="fw-bold fs-5">@card.Rank</div>
                             <div class="small">@card.Suit</div>
                        </div>
                    }
                </div>
            </div>
        </div>
    </div>

    <div class="mt-5 pt-4">
        <a asp-action="Compare" class="btn btn-success btn-lg px-5 shadow">Deal Showdown</a>
        <a asp-action="Index" class="btn btn-outline-secondary btn-lg ms-3">Back to Draw</a>
    </div>
</div>
```
This approach reuses your robust tie-breaking logic (`CompareTo` and `Strength` list) to ensure that if two combinations result in "One Pair", the one with the higher pair (or kickers) is correctly selected as the "Best Hand".