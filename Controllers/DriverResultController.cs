using mowlds.github.io.DAL;
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
            var sRLContext = _context.DriverResult.
                  Include("Driver1").
                  Include("Race1").
                  Include("Session").
                  Where(grandprix => grandprix.Race == race);
            return View(sRLContext);
        }

    }
}
