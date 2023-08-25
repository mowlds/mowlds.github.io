using mowlds.github.io.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace mowlds.github.io.Models
{
    public class DriverResultExtra : DriverResult
    {
        public DriverResultExtra(DriverResult driver)
        { 
            this.BestLap = driver.BestLap;
            this.Delta = driver.Delta;
            this.Driver = driver.Driver;
            this.Driver1 = driver.Driver1;
            this.FinalPosition = driver.FinalPosition;
            this.GridPenaltyDrop = driver.GridPenaltyDrop;
            this.HasDNF = driver.HasDNF;
            this.HasDriverOfTheDay = driver.HasDriverOfTheDay;
            this.HasDSQ = driver.HasDSQ;
            this.HasFastestLap = driver.HasFastestLap;
            this.HasGridBan = driver.HasGridBan;
            this.HasGridPenalty = driver.HasGridPenalty;
            this.HasTimePenalty = driver.HasTimePenalty;
            this.ID = driver.ID;
            this.IsClassified = driver.IsClassified;
            this.Race = driver.Race;
            this.Race1 = driver.Race1;
            this.RacePoints = driver.RacePoints;
            this.Session = driver.Session;
            this.SessionType = driver.SessionType;
            this.Time = driver.Time;
            this.TimePenaltyDuration = driver.TimePenaltyDuration;
            this.Tyre1 = driver.Tyre1;
            this.Tyre2 = driver.Tyre2;
            this.Tyre3 = driver.Tyre3;
            this.Tyre4 = driver.Tyre4;
            this.Tyre5 = driver.Tyre5;
            this.Tyre6 = driver.Tyre6;
            this.Tyre7 = driver.Tyre7;
            this.Tyre8 = driver.Tyre8;
        }

        public int PositionsGained { get; set; }
        public int NextRace { get; set; }
        public int PreviousRace { get; set; }
    }
}