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
            var allResults = _context.DriverResult.Include("Race1").Where(dr => dr.SessionType > 2).ToList();

            var driver = _context.Driver.Where(d => d.ID == driverID);
            var seasons = _context.Season.Include("DriverTeam").Include("DriverTeam.Team1");
            var races = _context.Race.Include("Track1").OrderBy(r => r.RaceNumber).ToList();
            var allTracks = _context.Track;
            var superGridContext = new List<DriverCareerResultModel>();
            foreach (var season in seasons)
            {
                var supergrid = new DriverCareerResultModel();
                supergrid.driver = driver.First();
                supergrid.season = season;
                supergrid.races = races.Where(r => r.Season == season.ID).OrderBy(r => r.RaceNumber).ToList();
                supergrid.driverResults = _context.DriverResult.Include("Race1").Where(d => d.Race1.Season == season.ID && d.Driver == driverID && d.SessionType == 3).ToList();
                supergrid.races = supergrid.races;
                supergrid.driverResults = supergrid.driverResults.OrderBy(dr => dr.Race1.Track1.Abbreviation).ToList();
                supergrid.totalPoints = supergrid.driverResults.Sum(d => d.RacePoints.HasValue ? d.RacePoints.Value : 0);
                supergrid.totalPoints = supergrid.totalPoints + allResults.Where(ar => ar.SessionType ==4 && ar.Driver == driverID && ar.Race1.Season == season.ID).Sum(d => d.RacePoints.HasValue ? d.RacePoints.Value : 0);
                supergrid.diffPoints = 0;
                supergrid.finalPosition = CalculateFinalPosition(allResults, supergrid.driver.ID, season.ID);
                supergrid.allTracks = allTracks.ToList();
                supergrid.highestPosition = 999;
                superGridContext.Add(supergrid);
            }

            return PartialView(superGridContext.OrderByDescending(sg => sg.season.Year).ThenByDescending(sg => sg.season.Number));
        }

        [Route("DriverSupergridQualyPartial")]
        public async Task<ActionResult> DriverSupergridQualyPartial(int driverID)
        {
            var allResults = _context.DriverResult.Include("Race1").Where(dr => dr.SessionType == 2).ToList();

            var driver = _context.Driver.Where(d => d.ID == driverID);
            var seasons = _context.Season.Include("DriverTeam").Include("DriverTeam.Team1");
            var races = _context.Race.Include("Track1").OrderBy(r => r.RaceNumber).ToList();
            var allTracks = _context.Track;
            var superGridContext = new List<DriverCareerSupergridModel>();
            foreach (var season in seasons)
            {

                var supergrid = new DriverCareerSupergridModel();
                supergrid.driver = driver.First();
                supergrid.season = season;
                supergrid.races = races.Where(r => r.Season == season.ID).OrderBy(r => r.RaceNumber).ToList();
                supergrid.driverResults = _context.DriverResult.Include("Race1").Where(d => d.Race1.Season == season.ID && d.Driver == driverID && d.SessionType == 2).ToList();
                supergrid.races = supergrid.races;
                supergrid.driverResults = supergrid.driverResults.OrderBy(dr => dr.Race1.Track1.Abbreviation).ToList();
                supergrid.finalPosition = CalculateFinalSupergrid(allResults, supergrid.driver.ID, season.ID);
                supergrid.allTracks = allTracks.ToList();
                supergrid.highestPosition = 999;
                superGridContext.Add(supergrid);
            }

            foreach (var supergrid in superGridContext)
            {
                supergrid.driverResults = supergrid.driverResults.OrderBy(dr => dr.Race1.RaceNumber).ToList();
                if (supergrid.driverResults.Any())
                {
                    supergrid.avgGridPosition = Math.Round(supergrid.driverResults.Average(dr => dr.FinalPosition), 2, MidpointRounding.AwayFromZero);
                }
            }

            superGridContext = superGridContext.OrderBy(sg => sg.avgGridPosition).ToList();
            int index = 0;
            foreach (var supergrid in superGridContext)
            {
                if (supergrid.driverResults.Any())
                {
                    index++;
                    supergrid.currentTablePosition = index;
                }
            }

            return PartialView(superGridContext.OrderByDescending(sg => sg.season.Year).ThenByDescending(sg => sg.season.Number));
        }

        private int CalculateFinalPosition(List<DriverResult> results, int driver, int season)
        {
            int result = 0;
            var seasonResults = results.Where(dr => dr.Race1.Season == season).ToList();
            Dictionary<int, int> driverPoints = new Dictionary<int, int>();
            var allDrivers = _context.Driver;
            foreach (var drivers in allDrivers)
            {
                driverPoints.Add(drivers.ID, seasonResults.Where(sr => sr.Driver == drivers.ID).Sum(sr => sr.RacePoints.HasValue ? sr.RacePoints : 0).Value);
            }
            var sortedresults =  driverPoints.OrderByDescending(dp => dp.Value).ToDictionary(dp => dp.Key, dp=> dp.Value);
            int i = 1;
            foreach (int d in sortedresults.Keys)
            {
                if (d == driver)
                {
                    break;
                }
                i++;
            }
            result = i;
            return result;
        }

        private int CalculateFinalSupergrid(List<DriverResult> results, int driver, int season)
        {
            int result = 0;
            var seasonResults = results.Where(dr => dr.Race1.Season == season).ToList();
            Dictionary<int, double> driverPoints = new Dictionary<int, double>();
            var allDrivers = _context.Driver;
            foreach (var drivers in allDrivers)
            {
                if (seasonResults.Where(sr => sr.Driver == drivers.ID).Any())
                { 
                    driverPoints.Add(drivers.ID, Math.Round(seasonResults.Where(sr => sr.Driver == drivers.ID).Average(sr => sr.FinalPosition), 2, MidpointRounding.AwayFromZero));
                }
            }
            var sortedresults = driverPoints.OrderBy(dp => dp.Value).ToDictionary(dp => dp.Key, dp => dp.Value);
            int i = 1;
            foreach (int d in sortedresults.Keys)
            {
                if (d == driver)
                {
                    break;
                }
                i++;
            }
            result = i;
            return result;
        }

    }
}