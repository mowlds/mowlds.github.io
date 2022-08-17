using mowlds.github.io.DAL;
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
            var sRLContext = _context.Track.Where(t => t.ID == track);
            return View(sRLContext.First());
        }
    }
}