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
    public class DriverController : Controller
    {
      
       private SRLContext1 _context = new SRLContext1();

        public DriverController()
        {
        }

        public DriverController(SRLContext1 context)
        {
            _context = context;
        }

        [Route("Drivers")]
        public async Task<ActionResult> Index()
        {

            var sRLContext = _context.Driver.
                OrderBy(d => d.Name);
            return View(sRLContext);
        }

        [Route("Driver")]
        public async Task<ActionResult> Driver(int driver)
        {
            var drivers = _context.Driver.Where(d => d.ID == driver);
            var driverModel = new DriverDisplayModel();

            driverModel.Driver = drivers.First();
            driverModel.DriverResults = _context.DriverResult.Include("Race1").Include("Race1.Season1").Where(dr => dr.Driver == driver).ToList();
            driverModel.Dotds = driverModel.DriverResults.Where(dr => dr.HasDriverOfTheDay && dr.SessionType == 3).Count();
            driverModel.Fls = driverModel.DriverResults.Where(dr => dr.HasFastestLap && dr.SessionType == 3).Count();
            driverModel.Podiums = driverModel.DriverResults.Where(dr => dr.FinalPosition <=3 && dr.SessionType == 3).Count();
            driverModel.Poles = driverModel.DriverResults.Where(dr => dr.FinalPosition ==1 && dr.SessionType == 2).Count();
            driverModel.TotalPoints = driverModel.DriverResults.Where(dr => dr.SessionType > 2).Sum(dr => dr.RacePoints).Value;
            driverModel.Wins = driverModel.DriverResults.Where(dr => dr.FinalPosition ==1 && dr.SessionType == 3).Count();
            driverModel.TotalRaces = driverModel.DriverResults.Where(dr => dr.SessionType == 3).Count();
            driverModel.DNFs = driverModel.DriverResults.Where(dr => dr.SessionType == 3 && dr.HasDNF).Count();
            driverModel.FirstRace = driverModel.DriverResults.OrderBy(dr => dr.Race1.RaceDate).First().Race1;

            return View(driverModel);
        }

        [Route("DriverCareerResults")]
        public async Task<ActionResult> DriverCareerResults(int driverID)
        {
            var driver = _context.Driver.Where(d => d.ID == driverID);
            var seasons = _context.Season.Include("DriverTeam").Include("DriverTeam.Team1");
            var races = _context.Race.Include("Track1").OrderBy(r => r.RaceNumber).ToList();
            var allTracks = _context.Track;
            var superGridContext = new List<DriverCareerResultModel>();
            //var sprints = _context.DriverResult.Include("Race1").Where(dr => dr.SessionType == 4 && dr.Driver == driverID);
            //List<int> sprintAdded = new List<int>();
            //foreach (var sprintrace in sprints)
            //{
            //    if (!sprintAdded.Contains(sprintrace.Race1.ID))
            //    {
            //        sprintAdded.Add(sprintrace.Race1.ID);
            //        races.Add(sprintrace.Race1);
            //    }
            //}
            int maxpoints = 0;
            foreach (var season in seasons)
            {
                var supergrid = new DriverCareerResultModel();
                supergrid.driver = driver.First();
                supergrid.season = season;
                supergrid.races = races.Where(r => r.Season == season.ID).OrderBy(r => r.RaceNumber).ToList();
                supergrid.driverResults = _context.DriverResult.Include("Race1").Where(d => d.Race1.Season == season.ID && d.Driver == driverID && d.SessionType == 3).ToList();
                supergrid.races = supergrid.races;
                //supergrid.driverResults.AddRange(sprints.Where(s => s.Driver == supergrid.driver.ID));
                supergrid.driverResults = supergrid.driverResults.OrderBy(dr => dr.Race1.Track1.Abbreviation).ToList();
                supergrid.totalPoints = supergrid.driverResults.Sum(d => d.RacePoints.HasValue ? d.RacePoints.Value : 0);
                supergrid.diffPoints = 0;
                supergrid.finalPosition = 0;
                supergrid.allTracks = allTracks.ToList();
                if (supergrid.driverResults.Any())
                {
                    supergrid.highestPosition = supergrid.driverResults.Min(dr => dr.FinalPosition);
                }
                else
                {
                    supergrid.highestPosition = 999;
                }
                superGridContext.Add(supergrid);
            }

            return PartialView(superGridContext.OrderByDescending(sg => sg.season.Year).ThenByDescending(sg => sg.season.Number));
        }

    }
}