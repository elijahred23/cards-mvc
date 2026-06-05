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
            var deck = new Deck();

            deck.Initialize();
            deck.Shuffle();


            var playerHole = Enumerable.Range(0, 2).Select(_ => deck.Deal()).ToList();
            var houseHole = Enumerable.Range(0, 2).Select(_ => deck.Deal()).ToList();

            var communityCards = Enumerable.Range(0, 5).Select(_ => deck.Deal()).ToList();


            var playerBest = PokerHandEvaluator.GetBestHand(playerHole.Concat(communityCards));
            var houseBest = PokerHandEvaluator.GetBestHand(houseHole.Concat(communityCards));


            int comparison = playerBest.CompareTo(houseBest);

            ViewBag.Message = comparison > 0 ? "Player Wins!" : (comparison < 0 ? "House wins!" : "Split Pot!");

            return View((Player: playerBest, House: houseBest, Community: communityCards, PlayerHole: playerHole, HouseHole: houseHole));
        }
    }
    
}