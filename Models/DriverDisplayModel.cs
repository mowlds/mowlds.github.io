using mowlds.github.io.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace mowlds.github.io.Models
{
    public class DriverDisplayModel
    {
        public Driver Driver { get; set; }
        public List<DriverResult> DriverResults { get; set; }

        public Race FirstRace { get; set; }

        public int TotalPoints { get; set; }

        public int Wins { get; set; }

        public int Podiums { get; set; }

        public int Poles { get; set; }

        public int Dotds { get; set; }

        public int Fls { get; set; }

        public int DNFs { get; set; }

        public int TotalRaces { get; set; }

    }
}