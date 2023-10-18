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
    public class HomeController : Controller
    {
        private SRLContext1 _context = new SRLContext1();
        public ActionResult Index()
        {
            return View();
        }

        private int CurrentSeasonDetails()
        {
            var sRLContext = _context.Season.
                  Include("Race").
                   OrderByDescending(s => s.GameVersion).ThenByDescending(s => s.Number);
            int currentSeason = sRLContext.First().ID;
            return currentSeason;
        }

        public async Task<ActionResult> TableNarrow(int seasonID)
        {

            if (seasonID == 0)
            {
                seasonID = CurrentSeasonDetails();
            }
            var drivers = _context.Driver;
            var season = _context.Season.Include("DriverTeam").Include("DriverTeam.Team1").Where(s => s.ID == seasonID);
            var races = _context.Race.Include("Track1").Where(r => r.Season == seasonID).OrderBy(r => r.RaceNumber).ToList();
            var superGridContext = new List<SupergridViewModel>();
            var sprints = _context.DriverResult.Include("Race1").Where(dr => dr.SessionType == 4 && dr.Race1.Season == seasonID);
            List<int> sprintAdded = new List<int>();
            foreach (var sprintrace in sprints)
            {
                if (!sprintAdded.Contains(sprintrace.Race1.ID))
                {
                    sprintAdded.Add(sprintrace.Race1.ID);
                    races.Add(sprintrace.Race1);
                }
            }
            int maxpoints = 0;
            foreach (var driver in drivers)
            {
                var supergrid = new SupergridViewModel();
                supergrid.driver = driver;
                supergrid.season = season.First();
                supergrid.races = races.OrderBy(r => r.RaceNumber).ToList();
                supergrid.driverResults = _context.DriverResult.Include("Race1").Where(d => d.Race1.Season == seasonID && d.Driver == driver.ID && d.SessionType == 3).ToList();
                supergrid.diffPoints = 0;
                supergrid.races = supergrid.races;
                supergrid.driverResults.AddRange(sprints.Where(s => s.Driver == supergrid.driver.ID));
                supergrid.driverResults = supergrid.driverResults.OrderBy(dr => dr.Race1.RaceNumber).ThenByDescending(dr => dr.SessionType).ToList();
                supergrid.totalPoints = supergrid.driverResults.Sum(d => d.RacePoints.HasValue ? d.RacePoints.Value : 0);
                if (supergrid.driverResults.Any())
                {
                    supergrid.highestPosition = supergrid.driverResults.Min(dr => dr.FinalPosition);
                }
                else
                {
                    supergrid.highestPosition = 999;
                }
                superGridContext.Add(supergrid);
                if (supergrid.totalPoints > maxpoints)
                {
                    maxpoints = supergrid.totalPoints;
                }
            }
            superGridContext = superGridContext.OrderByDescending(sg => sg.totalPoints).ThenBy(sg => sg.highestPosition).ToList();
            int index = 0;
            foreach (var supergrid in superGridContext)
            {
                index++;
                supergrid.diffPoints = supergrid.totalPoints - maxpoints;
                supergrid.currentTablePosition = index;
            }
            return PartialView(superGridContext);
        }
    }
}