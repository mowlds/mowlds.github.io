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
            int currentSeason = sRLContext.First().ID;
            var seasonChampion = new List<SeasonChampionModel>();
            foreach (Season s in sRLContext)
            {
                SeasonChampionModel sc = new SeasonChampionModel();
                sc.DriverTeam = s.DriverTeam;
                sc.GameVersion = s.GameVersion;
                sc.ID = s.ID;
                sc.isoneshotqualy = s.isoneshotqualy;
                sc.isrealperformance = s.isrealperformance;
                sc.isrealteams = s.isrealteams;
                sc.Number = s.Number;
                sc.Race = s.Race;
                sc.Year = s.Year;
                sc.Champion = currentSeason == sc.ID ? "---": GetChampion(sc.ID);
                seasonChampion.Add(sc);
            }

            seasonChampion = GetNumberOfTitles(seasonChampion);
            return View(seasonChampion);
        }

        private string GetChampion(int seasonID)
        {
            string champion = String.Empty;

            var srlContxt = _context.DriverResult.Include("Race1").Where(d => d.Race1.Season == seasonID && d.SessionType > 2).OrderBy(dr => dr.Driver).ToList();
            int driverpoints = 0;
            int winnerpoints = 0;
            int driver = 0;
            foreach (var dr in srlContxt)
            {
                if (dr.Driver != driver)
                {
                    driver = dr.Driver;
                    driverpoints  = srlContxt.Where(drive => drive.Driver == dr.Driver).Sum(d=> d.RacePoints.HasValue ? d.RacePoints.Value: 0);
                    if (driverpoints > winnerpoints)
                    {
                        winnerpoints = driverpoints;
                        champion = dr.Driver1.Name;
                    }
                }
            }

            return champion;
        }

        private List<SeasonChampionModel> GetNumberOfTitles(List<SeasonChampionModel> lst)
        {
            lst.Reverse(); //list is descending, we want to turn it around.

            Dictionary<string, int> championsDict = new Dictionary<string, int>();
            foreach (SeasonChampionModel scm in lst)
            {
                if (championsDict.ContainsKey(scm.Champion))
                {
                    championsDict[scm.Champion] = championsDict[scm.Champion] + 1;
                }
                else
                {
                    championsDict.Add(scm.Champion, 1); //new add always on 1
                }

                scm.NumberOfTitles = championsDict[scm.Champion];
            }

            lst.Reverse(); //reverse again.
            return lst;
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
                  Include("DriverTeam.Team1").
                  Include("Race.DriverResult").
                  Where(s => s.ID == season);
            ViewData["Champion"] = GetChampion(season);
            return View(sRLContext);
        }

        private int CurrentSeasonDetails()
        {
            var sRLContext = _context.Season.
                  Include("Race").
                   OrderByDescending(s => s.GameVersion).ThenByDescending(s => s.Number);
            int currentSeason = sRLContext.First().ID;
            return currentSeason;
        }

        [Route("SeasonDetails")]
        public async Task<ActionResult> SeasonDetails(int season)
        {
            if (season == 0)
            {
                season = CurrentSeasonDetails();
            }
            //Collect the results for a particular race and all related tables and return it as a view.
            //This will get passed on to the partials in each page.
            var sRLContext = _context.Season.
                  Include("Race").
                  Include("Race.Track1").
                  Include("DriverTeam").
                  Include("DriverTeam.Team1").
                  Include("Race.DriverResult").
                  Where(s => s.ID == season);
            ViewData["Champion"] = GetChampion(season);
            return View(sRLContext);
        }

        [Route("SupergridPartial")]
        public async Task<ActionResult> SupergridPartial(int seasonID)
        {
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

        [Route("SupergridQualyPartial")]
        public async Task<ActionResult> SupergridQualyPartial(int seasonID)
        {
            var drivers = _context.Driver;
            var season = _context.Season.Include("DriverTeam").Include("DriverTeam.Team1").Where(s => s.ID == seasonID);
            var races = _context.Race.Include("Track1").Where(r => r.Season == seasonID).OrderBy(r => r.RaceNumber).ToList();
            var superGridContext = new List<SuperGridQualyModel>();
            
            foreach (var driver in drivers)
            {
                var supergrid = new SuperGridQualyModel();
                supergrid.driver = driver;
                supergrid.season = season.First();
                supergrid.races = races.OrderBy(r => r.RaceNumber).ToList();
                supergrid.driverResults = _context.DriverResult.Include("Race1").Where(d => d.Race1.Season == seasonID && d.Driver == driver.ID && d.SessionType == 2).ToList();
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
        public async Task<ActionResult> ConstructorsTable(int seasonID)
        {
            var drivers = _context.Driver.Include("DriverTeam");
            var teams = _context.Team;
            var season = _context.Season.Include("DriverTeam").Include("DriverTeam.Team1").Where(s => s.ID == seasonID);
            var constructorTable = new List<ConstructorsModel>();

            foreach (var team in teams)
            {
                if (season.First().DriverTeam.Any(dt=> dt.Team == team.ID))
                {
                    var constructor = new ConstructorsModel();
                    constructor.driver1 = season.First().DriverTeam.Where(dt => dt.Team == team.ID).FirstOrDefault().Driver1;
                    if (season.First().DriverTeam.Where(dt => dt.Team == team.ID).Count() > 1)
                    { 
                        constructor.driver2 = season.First().DriverTeam.Where(dt => dt.Team == team.ID).LastOrDefault().Driver1;
                    }
                    
                    constructor.team = team;
                    //race wins only, so get all race results first.
                    var driverResult = _context.DriverResult.Include("Race1").Where(d => d.Race1.Season == seasonID && d.Driver == constructor.driver1.ID && d.SessionType ==3).ToList();
                    if (constructor.driver2 != null)
                    {     
                        driverResult.AddRange(_context.DriverResult.Include("Race1").Where(d => d.Race1.Season == seasonID && d.Driver == constructor.driver2.ID && d.SessionType == 3).ToList());
                    }
                    var totalWins = driverResult.Count(dr => dr.FinalPosition == 1);
                    //merge in sprint results to get total points.
                    driverResult.AddRange(_context.DriverResult.Include("Race1").Where(d => d.Race1.Season == seasonID && d.Driver == constructor.driver1.ID && d.SessionType == 4).ToList());
                    if (constructor.driver2 != null)
                    {
                        driverResult.AddRange(_context.DriverResult.Include("Race1").Where(d => d.Race1.Season == seasonID && d.Driver == constructor.driver2.ID && d.SessionType == 4).ToList());
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