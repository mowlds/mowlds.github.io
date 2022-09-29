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
    public class StatsController : Controller
    {
        private SRLContext1 _context = new SRLContext1();
        // GET: Stats
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> TrackStats(int track)
        {
            var drivers = _context.Driver.ToList();
            var result = _context.DriverResult.Include("Race1").Include("Race1.Season1").Include("Driver1").Where(dr => dr.Race1.Track == track);

            List<StatsModel> returnValue = new List<StatsModel>();

            foreach (Driver d in drivers)
            {
                var driverresult = result.Where(dr => dr.Driver == d.ID);
                returnValue.Add(ConvertToStats(d.Name, driverresult.ToList()));
            }

            returnValue = returnValue.OrderByDescending(rv => rv.RaceWins).ThenByDescending(rv => rv.RaceStarts).ToList();

            return PartialView("Stats", returnValue);
        }

        public async Task<ActionResult> DriverStats(int driver)
        {
            var tracks = _context.Track.ToList();
            Driver d = _context.Driver.Where(dr => dr.ID == driver).First();
            var result = _context.DriverResult.Include("Race1").Include("Race1.Track1").Include("Race1.Season1").Include("Driver1").Where(dr => dr.Driver == driver);

            List<StatsModel> returnValue = new List<StatsModel>();

            foreach (Track t in tracks)
            {
                var driverresult = result.Where(dr => dr.Race1.Track == t.ID);
                returnValue.Add(ConvertToStats(t.Abbreviation, driverresult.ToList()));
            }

            returnValue = returnValue.OrderByDescending(rv => rv.RaceWins).ThenByDescending(rv => rv.RaceStarts).ToList();

            return PartialView("Stats", returnValue);

        }

        //public async Task<ActionResult> TotalStats()
        //{
        //    var result = _context.DriverResult.Include("Race1").Include("Race1.Season1").Include("Driver1")
        //}


        private StatsModel ConvertToStats(string name, List<DriverResult> driverResults)
        {
            StatsModel driverStat = new StatsModel();
            driverStat.RaceStarts = driverResults.Where(dr => dr.SessionType == 3).Count();
            driverStat.BestFinish = 0;
            driverStat.WorstFinish = 0;
            if (driverResults.Where(dr => dr.SessionType == 3).Any())
            {
                driverStat.BestFinish = driverResults.Where(dr => dr.SessionType == 3).Min(dr => dr.FinalPosition);
                driverStat.WorstFinish = driverResults.Where(dr => dr.SessionType == 3).Max(dr => dr.FinalPosition);
            }
            driverStat.DNF = driverResults.Where(dr => dr.SessionType == 3 && dr.HasDNF).Count();
            driverStat.Name = name;
            driverStat.DriveroftheDay = driverResults.Where(dr => dr.HasDriverOfTheDay && dr.SessionType == 3).Count();
            driverStat.FastestLaps = driverResults.Where(dr => dr.HasFastestLap && dr.SessionType == 3).Count();
            driverStat.Podiums = driverResults.Where(dr => dr.FinalPosition <= 3 && dr.SessionType == 3).Count();
            driverStat.PointsFinish = driverResults.Where(dr => dr.RacePoints > 0 && dr.SessionType == 3).Count();
            driverStat.Poles = driverResults.Where(dr => dr.FinalPosition == 1 && dr.SessionType == 2).Count();
            driverStat.QualyDSQ = driverResults.Where(dr => dr.HasDSQ && dr.SessionType == 2).Count();
            driverStat.RaceDSQ = driverResults.Where(dr => dr.HasDSQ && dr.SessionType == 3).Count();
            driverStat.RaceWins = driverResults.Where(dr => dr.FinalPosition == 1 && dr.SessionType == 3).Count();
            driverStat.TotalPoints = driverResults.Where(dr => dr.SessionType > 2).Sum(dr => dr.RacePoints).Value;

            if (driverStat.RaceStarts > 0)
            { 
                driverStat.AvgPoints = Math.Round((driverStat.TotalPoints / (decimal)driverStat.RaceStarts), 2);
                driverStat.DNFPerc = Math.Round((decimal)driverStat.DNF / (decimal)driverStat.RaceStarts * 100, 2);
                driverStat.DriveroftheDayPerc = Math.Round((decimal)driverStat.DriveroftheDay / (decimal)driverStat.RaceStarts * 100, 2);
                driverStat.FastestLapsPerc = Math.Round((decimal)driverStat.FastestLaps / (decimal)driverStat.RaceStarts * 100, 2);
                driverStat.PodiumPerc = Math.Round((decimal)driverStat.Podiums / (decimal)driverStat.RaceStarts * 100, 2);
                driverStat.PointsFinishPerc = Math.Round((decimal)driverStat.PointsFinish / (decimal)driverStat.RaceStarts * 100, 2);
                driverStat.PolePerc = Math.Round((decimal)driverStat.Poles / (decimal)driverStat.RaceStarts * 100, 2);
                driverStat.RaceWinPerc = Math.Round((decimal)driverStat.RaceWins / (decimal)driverStat.RaceStarts * 100, 2);
            }
            else
            {
                driverStat.AvgPoints = 0;
                driverStat.DNFPerc = 0;
                driverStat.DriveroftheDayPerc = 0;
                driverStat.FastestLapsPerc = 0;
                driverStat.PodiumPerc = 0;
                driverStat.PointsFinishPerc = 0;
                driverStat.PolePerc = 0;
                driverStat.RaceWinPerc = 0;
            }


            return driverStat;
        }
    }
}