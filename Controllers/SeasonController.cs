using mowlds.github.io.DAL;
using mowlds.github.io.Models;
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

        [Route("SupergridPartial")]
        public async Task<ActionResult> SupergridPartial(int seasonID)
        {
            var drivers = _context.Driver;
            var season = _context.Season.Where(s=> s.ID == seasonID);
            var races = _context.Race.Include("Track1").Where(r => r.Season == seasonID).OrderBy(r => r.RaceNumber);
            var superGridContext = new List<SupergridViewModel>();
            
            foreach (var driver in drivers)
            {
                var supergrid = new SupergridViewModel();
                supergrid.driver = driver;
                supergrid.season = season.First();
                supergrid.races = races.ToList();
                supergrid.driverResults = _context.DriverResult.Include("Race1").Where(d => d.Race1.Season == seasonID && d.Driver == driver.ID && d.SessionType > 2).ToList();
                supergrid.totalPoints = supergrid.driverResults.Sum(d => d.RacePoints.HasValue ? d.RacePoints.Value : 0);
                supergrid.diffPoints = 0;
                superGridContext.Add(supergrid);
            }
            return PartialView(superGridContext.OrderByDescending(sg => sg.totalPoints));
        }


    }
}