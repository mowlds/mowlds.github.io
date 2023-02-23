using mowlds.github.io.DAL;
using mowlds.github.io.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
            StatsViewModel svm = new StatsViewModel();
            svm.Drivers = _context.Driver.OrderBy(d=> d.Name).ToList();
            svm.Seasons = _context.Season.ToList();
            svm.Sessions = _context.Session.ToList();
            svm.Tracks = _context.Track.OrderBy(t=>t.Name).ToList();
            List<GameVersion> version = new List<GameVersion>();
            foreach (Season s in svm.Seasons)
            {
                string gameversion = s.GameVersion.Trim();
                if (!version.Any(v => v.version == gameversion))
                {
                    GameVersion v = new GameVersion();
                    v.version = gameversion;
                    version.Add(v);
                }   
            }
            svm.GameVersion = version.OrderBy(v => v.version).ToList();
            return View(svm);
        }

        public async Task<ActionResult> AllStats(int driver, int track, StatType StatType, string version, int session, string sortorder)
        {
            var drivers = _context.Driver.ToList();
            var tracks = _context.Track.ToList();
            var result = _context.DriverResult.Include("Race1").Include("Race1.Track1").Include("Race1.Season1").Include("Driver1").ToList();
            if (driver > 0)
            {
                result = result.Where(dr => dr.Driver == driver).ToList();
                drivers = drivers.Where(d => d.ID == driver).ToList();
            }
            if (track > 0)
            {
                result = result.Where(dr => dr.Race1.Track == track).ToList();
                tracks = tracks.Where(t => t.ID == track).ToList();
            }
            if (StatType != StatType.All)
            {
                if (StatType == StatType.EqualPerformance)
                {
                    result = result.Where(dr => !dr.Race1.Season1.isrealperformance).ToList();
                }
                else if (StatType == StatType.RealPerformance)
                {
                    result = result.Where(dr => dr.Race1.Season1.isrealperformance).ToList();
                }
            }
            if (version != "0")
            {
                result = result.Where(dr => dr.Race1.Season1.GameVersion.Trim() == version).ToList();
            }
            if (session > 0)
            {
                result = result.Where(dr => dr.SessionType == session).ToList();
            }

            List<StatsModel> returnValue = new List<StatsModel>();

            foreach (Driver d in drivers)
            {
                var driverresult = result.Where(dr => dr.Driver == d.ID);
                returnValue.Add(ConvertToStats(d.Name, driverresult.ToList(), session));
            }

            //TODO: Improve sorting performance by not refetching the data for every sort.
            returnValue = returnValue.OrderByDescending(rv => rv.GetType().GetProperty(sortorder).GetValue(rv, null)).ThenByDescending(rv => rv.RaceStarts).ToList();
            return PartialView("Stats", returnValue);
        }

        public async Task<ActionResult> TrackStats(int track, StatType StatType)
        {
            var drivers = _context.Driver.ToList();
            var result = _context.DriverResult.Include("Race1").Include("Race1.Season1").Include("Driver1").Where(dr => dr.Race1.Track == track);

            if (StatType == StatType.EqualPerformance)
            {
                result = result.Where(dr => !dr.Race1.Season1.isrealperformance);
            }
            else if (StatType == StatType.RealPerformance)
            {
                result = result.Where(dr => dr.Race1.Season1.isrealperformance);
            }
            else
            {
                //do not filter
            }

            List<StatsModel> returnValue = new List<StatsModel>();

            foreach (Driver d in drivers)
            {
                var driverresult = result.Where(dr => dr.Driver == d.ID);
                returnValue.Add(ConvertToStats(d.Name, driverresult.ToList(), 3));
            }

            returnValue = returnValue.OrderByDescending(rv => rv.RaceWins).ThenByDescending(rv => rv.RaceStarts).ToList();

            return PartialView("Stats", returnValue);
        }

        public async Task<ActionResult> DriverStats(int driver, StatType StatType)
        {
            var tracks = _context.Track.ToList();
            Driver d = _context.Driver.Where(dr => dr.ID == driver).First();
            var result = _context.DriverResult.Include("Race1").Include("Race1.Track1").Include("Race1.Season1").Include("Driver1").Where(dr => dr.Driver == driver);

            if (StatType == StatType.EqualPerformance)
            {
                result = result.Where(dr => !dr.Race1.Season1.isrealperformance);
            }
            else if (StatType == StatType.RealPerformance)
            {
                result = result.Where(dr => dr.Race1.Season1.isrealperformance);
            }
            else
            {
                //do not filter
            }

            List<StatsModel> returnValue = new List<StatsModel>();

            foreach (Track t in tracks)
            {
                var driverresult = result.Where(dr => dr.Race1.Track == t.ID);
                returnValue.Add(ConvertToStats(t.Abbreviation, driverresult.ToList(), 3));
            }

            returnValue = returnValue.OrderByDescending(rv => rv.RaceWins).ThenByDescending(rv => rv.RaceStarts).ToList();

            return PartialView("Stats", returnValue);

        }

        private StatsModel ConvertToStats(string name, List<DriverResult> driverResults, int sessionType)
        {
            if (sessionType == 0)
            {
                sessionType = 3;
            }
            StatsModel driverStat = new StatsModel();
            driverStat.RaceStarts = driverResults.Where(dr => dr.SessionType == sessionType).Count();
            driverStat.BestFinish = 0;
            driverStat.WorstFinish = 0;
            if (driverResults.Where(dr => dr.SessionType == sessionType).Any())
            {
                driverStat.BestFinish = driverResults.Where(dr => dr.SessionType == sessionType).Min(dr => dr.FinalPosition);
                driverStat.WorstFinish = driverResults.Where(dr => dr.SessionType == sessionType).Max(dr => dr.FinalPosition);
            }
            driverStat.DNF = driverResults.Where(dr => dr.SessionType == sessionType && dr.HasDNF).Count();
            driverStat.Name = name;
            driverStat.DriveroftheDay = driverResults.Where(dr => dr.HasDriverOfTheDay && dr.SessionType == sessionType).Count();
            driverStat.FastestLaps = driverResults.Where(dr => dr.HasFastestLap && dr.SessionType == sessionType).Count();
            driverStat.Podiums = driverResults.Where(dr => dr.FinalPosition <= 3 && dr.SessionType == sessionType).Count();
            driverStat.PointsFinish = driverResults.Where(dr => dr.RacePoints > 0 && dr.SessionType == sessionType).Count();
            driverStat.Poles = driverResults.Where(dr => dr.FinalPosition == 1 && dr.SessionType == 2).Count();
            driverStat.QualyDSQ = driverResults.Where(dr => dr.HasDSQ && dr.SessionType == 2).Count();
            driverStat.RaceDSQ = driverResults.Where(dr => dr.HasDSQ && dr.SessionType == sessionType).Count();
            driverStat.RaceWins = driverResults.Where(dr => dr.FinalPosition == 1 && dr.SessionType == sessionType).Count();
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