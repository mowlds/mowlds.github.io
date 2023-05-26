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
    public class TeamController : Controller
    {
        private SRLContext1 _context = new SRLContext1();

        public TeamController()
        {
        }

        public TeamController(SRLContext1 context)
        {
            _context = context;
        }

        [Route("Teams")]
        public async Task<ActionResult> Index()
        {

            var sRLContext = _context.Team.
                OrderBy(d => d.Name);
            return View(sRLContext);
        }


        [Route("Team")]
        public async Task<ActionResult> Team(int team)
        {
            var teams = _context.Team.Where(t => t.ID == team);
            var teamModel = new TeamDisplayModel();

            teamModel.Team = teams.First();
            teamModel.DriverTeams = _context.DriverTeam.Where(dt => dt.Team == team).ToList();
            teamModel.Driver = new List<Driver>();
            teamModel.DriverResults = new List<DriverResult>();
            foreach (DriverTeam dt in teamModel.DriverTeams)
            {
                teamModel.Driver.Add(_context.Driver.Where(d => d.ID == dt.Driver).First());
                teamModel.DriverResults.AddRange(_context.DriverResult.Include("Race1").Include("Race1.Season1").Where(dr => dr.Driver == dt.Driver && dr.Race1.Season == dt.Season).ToList());
                
            }

            teamModel.Dotds = teamModel.DriverResults.Where(dr => dr.HasDriverOfTheDay && dr.SessionType == 3).Count();
            teamModel.Fls = teamModel.DriverResults.Where(dr => dr.HasFastestLap && dr.SessionType == 3).Count();
            teamModel.Podiums = teamModel.DriverResults.Where(dr => dr.FinalPosition <= 3 && dr.SessionType == 3).Count();
            teamModel.Poles = teamModel.DriverResults.Where(dr => dr.FinalPosition == 1 && dr.SessionType == 2).Count();
            teamModel.TotalPoints = teamModel.DriverResults.Where(dr => dr.SessionType > 2).Sum(dr => dr.RacePoints).Value;
            teamModel.Wins = teamModel.DriverResults.Where(dr => dr.FinalPosition == 1 && dr.SessionType == 3).Count();
            teamModel.TotalRaces = teamModel.DriverResults.Where(dr => dr.SessionType == 3).GroupBy(dr => dr.Race).Count();
            teamModel.DNFs = teamModel.DriverResults.Where(dr => dr.SessionType == 3 && dr.HasDNF).Count();
            teamModel.FirstRace = teamModel.DriverResults.OrderBy(dr => dr.Race1.RaceDate).First().Race1;

            return View(teamModel);
        }

    }
}