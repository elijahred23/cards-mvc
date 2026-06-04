# Deck of Cards MVC Application Implementation

This document provides a complete implementation for a simple Deck of Cards application.

## 1. Models

### Card.cs
Define the properties of a single playing card.

```csharp
namespace CardsMvc.Models
{
    public enum Suit { Hearts, Diamonds, Clubs, Spades }
    public enum Rank { Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace }

    public class Card
    {
        public Suit Suit { get; set; }
        public Rank Rank { get; set; }
        public string DisplayName => $"{Rank} of {Suit}";
    }
}
```

### Deck.cs
Logic for creating, shuffling, and dealing from a collection of cards.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

namespace CardsMvc.Models
{
    public class Deck
    {
        public List<Card> Cards { get; set; } = new();

        public Deck() => Initialize();

        public void Initialize()
        {
            Cards.Clear();
            foreach (Suit s in Enum.GetValues(typeof(Suit)))
            {
                foreach (Rank r in Enum.GetValues(typeof(Rank)))
                {
                    Cards.Add(new Card { Suit = s, Rank = r });
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
            if (!Cards.Any()) return null;
            var card = Cards[0];
            Cards.RemoveAt(0);
            return card;
        }
    }
}
```

## 2. Controller

### DeckController.cs
Handles the web actions for viewing the deck, shuffling, and dealing.

```csharp
using Microsoft.AspNetCore.Mvc;
using CardsMvc.Models;

namespace CardsMvc.Controllers
{
    public class DeckController : Controller
    {
        private static readonly Deck _deck = new();

        public IActionResult Index() => View(_deck);

        [HttpPost]
        public IActionResult Shuffle() { _deck.Shuffle(); return RedirectToAction(nameof(Index)); }

        [HttpPost]
        public IActionResult Deal() { TempData["Dealt"] = _deck.Deal()?.DisplayName; return RedirectToAction(nameof(Index)); }

        [HttpPost]
        public IActionResult Reset() { _deck.Initialize(); return RedirectToAction(nameof(Index)); }
    }
}
```

## 3. View

### Index.cshtml
The Bootstrap-styled interface.

```cs
@model CardsMvc.Models.Deck
<div class="container text-center py-5">
    <h1 class="display-4 mb-4">MVC Deck of Cards</h1>
    <p class="h4">Remaining Cards: <span class="badge bg-secondary">@Model.Cards.Count</span></p>
    @if (TempData["Dealt"] != null) { <div class="alert alert-info mt-3">You just dealt: <strong>@TempData["Dealt"]</strong></div> }
    <div class="mt-4 mb-5">
        <form asp-action="Shuffle" method="post" class="d-inline"><button class="btn btn-primary btn-lg mx-1">Shuffle</button></form>
        <form asp-action="Deal" method="post" class="d-inline"><button class="btn btn-success btn-lg mx-1" @(Model.Cards.Count == 0 ? "disabled" : "")>Deal Card</button></form>
        <form asp-action="Reset" method="post" class="d-inline"><button class="btn btn-danger btn-lg mx-1">Reset</button></form>
    </div>
    <div class="row g-3 justify-content-center">
        @foreach (var card in Model.Cards) { <div class="col-auto"><div class="card shadow-sm border-dark" style="width: 120px; height: 160px; display: flex; align-items: center; justify-content: center; background-color: #f8f9fa;">@card.DisplayName</div></div> }
    </div>
</div>
```