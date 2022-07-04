using mowlds.github.io.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace mowlds.github.io.Controllers
{
    public class RacesController : Controller
    {
        private SRLContext1 _context = new SRLContext1();

        public RacesController()
        {
        }

        public RacesController(SRLContext1 context)
        {
            _context = context;
        }

        [Route("Races")]
        public async Task<ActionResult> Index()
        {

            var sRLContext = _context.Race.
                  Include("Season1").
                  Include("Track1").
                OrderByDescending(r => r.Season1.GameVersion).ThenByDescending(r => r.Season1.Number).ThenBy(r => r.RaceNumber);
            return View(sRLContext);
        }
    }
}