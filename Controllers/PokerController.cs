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
    }
}