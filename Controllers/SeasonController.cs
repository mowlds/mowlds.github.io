using mowlds.github.io.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace mowlds.github.io.Controllers
{
    public class SeasonController : Controller
    {
        private SRLContext1 _context = new SRLContext1();

        public SeasonController()
        {
        }


        public SeasonController(SRLContext1 context)
        {
            _context = context;
        }


        // GET: DriverResults
        [Route("Season")]
        public async Task<ActionResult> Index()
        {
            //Collect the results for a particular race and all related tables and return it as a view.
            //This will get passed on to the partials in each page.
            var sRLContext = _context.Season.
                  Include("Race").
                   OrderByDescending(s => s.GameVersion).ThenByDescending(s => s.Number);
            return View(sRLContext);
        }

        [Route("SeasonOverview")]
        public async Task<ActionResult> Season(int season)
        {
            //Collect the results for a particular race and all related tables and return it as a view.
            //This will get passed on to the partials in each page.
            var sRLContext = _context.Season.
                  Include("Race").
                  Include("Race.Track1").
                  Include("DriverTeam").
                  Include("Race.DriverResult").
                  Where(s => s.ID == season);

            return View(sRLContext);
        }
    }
}