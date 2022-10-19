using mowlds.github.io.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace mowlds.github.io.Models
{
    public class StatsViewModel
    {
        public List<Track> Tracks { get; set; }

        public List<Driver> Drivers { get; set; }

        public List<Season> Seasons { get; set; }

        public List<GameVersion> GameVersion { get; set; }

        public List<Session> Sessions {get; set;}


    }
}