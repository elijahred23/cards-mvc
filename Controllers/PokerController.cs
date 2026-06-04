using Microsoft.AspNetCore.Mvc;
using CardsMvc.Models;
using CardsMvc.Logic;
using System.Collections.Generic;

namespace CardsMvc.Controllers
{
    public class PokerController : Controller
    {
        private static readonly Deck _deck = new();

        public IActionResult Index()
        {
            _deck.Initialize();
            _deck.Shuffle();

            var hand = new List<Card>();

            for(int i = 0; i < 5; i++)
            {
                var card = _deck.Deal();

                if(card != null) hand.Add(card);
            }

            var rank = PokerHandEvaluator.DetermineRank(hand);


            ViewBag.Rank = rank.ToString();

            return View(hand);
        }
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
    }
    
}