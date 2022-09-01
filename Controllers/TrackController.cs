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
    public class TrackController : Controller
    {
        private SRLContext1 _context = new SRLContext1();

        public TrackController()
        {
        }


        public TrackController(SRLContext1 context)
        {
            _context = context;
        }


        [Route("Track")]
        public async Task<ActionResult> Index()
        {
            var sRLContext = _context.Track;
            return View(sRLContext);
        }

        [Route("Track")]
        public async Task<ActionResult> Track(int track)
        {
            var Track = _context.Track.Where(t => t.ID == track);
            var results = _context.DriverResult.Include("Race1").Include("Race1.Season1").Include("Driver1").Where(dr => dr.Race1.Track == track);
            results = results.OrderByDescending(dr => dr.Race1.Season1.Year).ThenByDescending(dr => dr.Race1.Season1.Number);

            var trackContext = new TrackHistoryModel();
            trackContext.AllRaces = results.ToList();
            trackContext.Track = Track.First();
            DriverResult qualyDriver = new DriverResult();
            DriverResult raceDriver = new DriverResult();
            DateTime qualyTime = DateTime.MaxValue;
            DateTime raceTime = DateTime.MaxValue;

            foreach (DriverResult dr in results)
            {
                if (dr.SessionType == 2 && dr.FinalPosition == 1)
                {
                    if (dr.Time != null)
                    { 
                        DateTime testTime = CreateTime(dr.Time.Trim());

                        if (qualyTime > testTime)
                        {
                            qualyTime = testTime;
                            qualyDriver = dr;
                        }
                    }
                }
                else if (dr.SessionType == 3 && dr.HasFastestLap)
                {
                    if (dr.BestLap != null)
                    {
                        DateTime testTime = CreateTime(dr.BestLap.Trim());

                        if (raceTime > testTime)
                        {
                            raceTime = testTime;
                            raceDriver = dr;
                        }
                    }
                }
            }
                
            trackContext.LapRecordQualy = qualyTime.ToString("m:ss:fff");
            trackContext.LapRecordQualyHolder = qualyDriver;
            trackContext.LapRecordRace = raceTime.ToString("m:ss:fff");
            trackContext.LapRecordRaceHolder = raceDriver;


            return View(trackContext);
        }

        public DateTime CreateTime(string time)
        {
            time = time.Replace(".", ":");
            string[] times = time.Split(':');
            DateTime dt = DateTime.MinValue;
            dt = dt.AddMinutes(int.Parse(times[0]));
            dt = dt.AddSeconds(int.Parse(times[1]));
            dt = dt.AddMilliseconds(int.Parse(times[2]));
            return dt;
        }
    }
}