using mowlds.github.io.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace mowlds.github.io.Models
{
    public class StatsModel
    {
        public string Name { get; set; }

        public int RaceStarts { get; set; }

        public int RaceWins { get; set; }

        public decimal RaceWinPerc { get; set; }

        public int Podiums { get; set; }

        public decimal PodiumPerc { get; set; }

        public int BestFinish { get; set; }

        public int WorstFinish { get; set; }

        public int PointsFinish { get; set; }

        public decimal PointsFinishPerc { get; set; }

        public int DNF { get; set; }

        public decimal DNFPerc { get; set; }

        public int Poles { get; set; }

        public decimal PolePerc { get; set; }

        public int FastestLaps { get; set; }

        public decimal FastestLapsPerc { get; set; }

        public int DriveroftheDay { get; set; }

        public decimal DriveroftheDayPerc { get; set; }

        public int TotalPoints { get; set; }

        public decimal AvgPoints { get; set; }

        public int RaceDSQ { get; set; }

        public int QualyDSQ { get; set; }
    }
}