using Microsoft.AspNetCore.Mvc;
using CardsMvc.Models;

namespace CardsMvc.Controllers
{
    public class DeckController : Controller
    {
        private static readonly Deck _deck = new();

        public IActionResult Index() => View(_deck);


        [HttpPost]
        public IActionResult Shuffle() {_deck.Shuffle(); 
        return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Deal()
        {
            TempData["Dealt"] = _deck.Deal()?.DisplayName; return
            RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Reset() {_deck.Initialize(); return RedirectToAction(nameof(Index));}
    }
}