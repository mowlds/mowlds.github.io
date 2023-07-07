using mowlds.github.io.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace mowlds.github.io.Controllers
{
    public class LiveController : Controller
    {
        // GET: Live
        private SRLContext1 _context = new SRLContext1();

        public LiveController()
        {
        }

        public LiveController(SRLContext1 context)
        {
            _context = context;
        }

        [Route("Live")]
        public async Task<ActionResult> Index()
        {
            var sRLContext = _context.Driver.Where(d => d.TwitchName != null && d.TwitchName != string.Empty).
                OrderBy(d => d.Name);
            var result = new List<Driver>();
            var twitch = "https://www.twitch.tv/";
            foreach (Driver d in sRLContext)
            {
                using (var client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(twitch + d.TwitchName);
                    var content = response.Content.ReadAsStringAsync();
                    if (content.Result.Contains("isLiveBroadcast"))
                    {
                        result.Add(d);
                    }
                }
                Thread.Sleep(500);
            }

            return View(result);
        }
    }
}