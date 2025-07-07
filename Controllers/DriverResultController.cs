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
    public class DriverResultController : Controller
    {
        private SRLContext1 _context = new SRLContext1();

        public DriverResultController()
        {
        }


        public DriverResultController(SRLContext1 context)
        {
            _context = context;
        }


        // GET: DriverResults
        [Route("DriverResults")]
        public async Task<ActionResult> Index(int? race)
        {
            //Collect the results for a particular race and all related tables and return it as a view.
            //This will get passed on to the partials in each page.

            var allRaces = _context.DriverResult;

            int counter = 1;
            var previousRace = 0;
            var nextRace = 0;


            while (previousRace == 0 || nextRace == 0)
            {
                if (allRaces.Where(r=> r.Race == race-counter).Any() && previousRace == 0)
                {
                    previousRace = allRaces.Where(r => r.Race == race - counter).First().Race;
                }
                if (allRaces.Where(r => r.Race == race + counter).Any() && nextRace == 0)
                {
                    nextRace = allRaces.Where(r => r.Race == race + counter).First().Race;
                }
                counter++;
                if (counter > 10)
                {
                    //CDB: while terminator, no further races found, just add one.
                    nextRace = (int)race+1;
                }
            }

            var sRLContext = allRaces.
                  Where(grandprix => grandprix.Race == race);

            var result = new List<DriverResultExtra>();
            foreach (DriverResult dr in sRLContext)
            {
                DriverResultExtra dre = new DriverResultExtra(dr);
                dre.NextRace = nextRace;
                dre.PreviousRace = previousRace;
                if (dr.SessionType == 3)
                {
                    var qualyResult = sRLContext.Where(q => q.Driver == dr.Driver && q.SessionType == 2).FirstOrDefault();
                    if (qualyResult != null)
                    {
                        dre.PositionsGained = qualyResult.FinalPosition - dre.FinalPosition;
                    }
                }
                result.Add(dre);
            }

            return View(result);
        }

        // GET: DriverResults
        [Route("RaceOverviewPartial")]
        public async Task<ActionResult> RaceOverviewPartial(int? race)
        {
            var sRLContext = _context.DriverResult.
               Include("Driver1").
               Include("Race1").
               Include("Race1.Season1").
               Where(grandprix => grandprix.Race == race);

            var result = new List<DriverResultExtra>();
            foreach (DriverResult dr in sRLContext)
            {
                DriverResultExtra dre = new DriverResultExtra(dr);
                result.Add(dre);
            }

            return PartialView(result);
        }

        // GET: DriverResults
        [Route("QualyPartial")]
        public async Task<ActionResult> QualyPartial(int? race)
        {
            var sRLContext = _context.DriverResult.
              Include("Driver1").
              Include("Race1").
              Include("Race1.Season1").Include("Race1.Season1.DriverTeam").Include("Race1.Season1.DriverTeam.Team1").
              Where(grandprix => grandprix.Race == race && grandprix.SessionType == 2);

            var result = new List<DriverResultExtra>();
            foreach (DriverResult dr in sRLContext)
            {
                DriverResultExtra dre = new DriverResultExtra(dr);
                result.Add(dre);
            }

            return PartialView(result);

        }

        // GET: DriverResults
        [Route("SprintPartial")]
        public async Task<ActionResult> SprintPartial(int? race)
        {
            var sRLContext = _context.DriverResult.
               Include("Driver1").
               Include("Race1").
               Include("Race1.Season1").Include("Race1.Season1.DriverTeam").Include("Race1.Season1.DriverTeam.Team1").
               Where(grandprix => grandprix.Race == race && grandprix.SessionType == 4);

            var result = new List<DriverResultExtra>();
            foreach (DriverResult dr in sRLContext)
            {
                DriverResultExtra dre = new DriverResultExtra(dr);
                result.Add(dre);
            }

            return PartialView(result);
        }

        // GET: DriverResults
        [Route("RacePartial")]
        public async Task<ActionResult> RacePartial(int? race)
        {
            var sRLContext = _context.DriverResult.
               Include("Driver1").
               Include("Race1").
               Include("Race1.Season1").Include("Race1.Season1.DriverTeam").Include("Race1.Season1.DriverTeam.Team1").
               Where(grandprix => grandprix.Race == race && grandprix.SessionType == 3);


            var qualyContext = _context.DriverResult.
               Include("Driver1").
               Include("Race1").
               Include("Race1.Season1").Include("Race1.Season1.DriverTeam").Include("Race1.Season1.DriverTeam.Team1").
               Where(grandprix => grandprix.Race == race && grandprix.SessionType == 2);


            var result = new List<DriverResultExtra>();
            foreach (DriverResult dr in sRLContext)
            {
                DriverResultExtra dre = new DriverResultExtra(dr);
                var qualyResult = qualyContext.Where(q => q.Driver == dr.Driver).FirstOrDefault();
                if (qualyResult != null)
                {
                    dre.PositionsGained = qualyResult.FinalPosition - dre.FinalPosition;
                }
                result.Add(dre);
            }
            return PartialView(result);
        }

        [Route("SupergridPartial")]
        public async Task<ActionResult> SupergridPartial(int seasonID, int raceNo)
        {
            var drivers = _context.Driver;
            var season = _context.Season.Include("DriverTeam").Include("DriverTeam.Team1").Where(s => s.ID == seasonID);
            var races = _context.Race.Include("Track1").Where(r => r.Season == seasonID && r.RaceNumber <= raceNo).OrderBy(r => r.RaceNumber).ToList();
            var superGridContext = new List<SupergridViewModel>();
            var sprints = _context.DriverResult.Include("Race1").Where(dr => dr.SessionType == 4 && dr.Race1.Season == seasonID && dr.Race1.RaceNumber <= raceNo);
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
                supergrid.driverResults = _context.DriverResult.Include("Race1").Where(d => d.Race1.Season == seasonID && d.Driver == driver.ID && d.SessionType == 3 && d.Race1.RaceNumber <= raceNo).ToList();
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

        [Route("SupergridQualyPartial")]
        public async Task<ActionResult> SupergridQualyPartial(int seasonID, int raceNo)
        {
            var drivers = _context.Driver;
            var season = _context.Season.Include("DriverTeam").Include("DriverTeam.Team1").Where(s => s.ID == seasonID);
            var races = _context.Race.Include("Track1").Where(r => r.Season == seasonID && r.RaceNumber <= raceNo).OrderBy(r => r.RaceNumber).ToList();
            var superGridContext = new List<SuperGridQualyModel>();

            foreach (var driver in drivers)
            {
                var supergrid = new SuperGridQualyModel();
                supergrid.driver = driver;
                supergrid.season = season.First();
                supergrid.races = races.OrderBy(r => r.RaceNumber).ToList();
                supergrid.driverResults = _context.DriverResult.Include("Race1").Where(d => d.Race1.Season == seasonID && d.Driver == driver.ID && d.SessionType == 2 && d.Race1.RaceNumber <= raceNo).ToList();
                supergrid.races = supergrid.races;
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

            return PartialView(superGridContext);
        }

        [Route("ConstructorsTable")]
        public async Task<ActionResult> ConstructorsTable(int seasonID, int raceNo)
        {
            var drivers = _context.Driver.Include("DriverTeam");
            var teams = _context.Team;
            var season = _context.Season.Include("DriverTeam").Include("DriverTeam.Team1").Where(s => s.ID == seasonID);
            var constructorTable = new List<ConstructorsModel>();

            foreach (var team in teams)
            {
                if (season.First().DriverTeam.Any(dt => dt.Team == team.ID))
                {
                    var constructor = new ConstructorsModel();
                    constructor.driver1 = season.First().DriverTeam.Where(dt => dt.Team == team.ID).FirstOrDefault().Driver1;
                    if (season.First().DriverTeam.Where(dt => dt.Team == team.ID).Count() > 1)
                    {
                        constructor.driver2 = season.First().DriverTeam.Where(dt => dt.Team == team.ID).LastOrDefault().Driver1;
                    }

                    constructor.team = team;
                    //race wins only, so get all race results first.
                    var driverResult = _context.DriverResult.Include("Race1").Where(d => d.Race1.Season == seasonID && d.Driver == constructor.driver1.ID && d.SessionType == 3 && d.Race1.RaceNumber <= raceNo).ToList();
                    if (constructor.driver2 != null)
                    {
                        driverResult.AddRange(_context.DriverResult.Include("Race1").Where(d => d.Race1.Season == seasonID && d.Driver == constructor.driver2.ID && d.SessionType == 3 && d.Race1.RaceNumber <= raceNo).ToList());
                    }
                    var totalWins = driverResult.Count(dr => dr.FinalPosition == 1);
                    //merge in sprint results to get total points.
                    driverResult.AddRange(_context.DriverResult.Include("Race1").Where(d => d.Race1.Season == seasonID && d.Driver == constructor.driver1.ID && d.SessionType == 4 && d.Race1.RaceNumber <= raceNo).ToList());
                    if (constructor.driver2 != null)
                    {
                        driverResult.AddRange(_context.DriverResult.Include("Race1").Where(d => d.Race1.Season == seasonID && d.Driver == constructor.driver2.ID && d.SessionType == 4 && d.Race1.RaceNumber <= raceNo).ToList());
                    }
                    var totalPoints = driverResult.Sum(d => d.RacePoints.HasValue ? d.RacePoints.Value : 0);


                    constructor.totalPoints = totalPoints;
                    constructor.totalWins = totalWins;
                    constructorTable.Add(constructor);
                }
            }

            constructorTable = constructorTable.OrderByDescending(ct => ct.totalPoints).ToList();
            int index = 0;
            foreach (var constructor in constructorTable)
            {
                index++;
                constructor.position = index;
            }


            return PartialView(constructorTable);
        }

    }
}
