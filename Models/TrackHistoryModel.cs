using mowlds.github.io.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace mowlds.github.io.Models
{
    public class TrackHistoryModel
    {
        public Track Track { get; set; }

        public List<DriverResult> AllRaces { get; set; }

        public string LapRecordQualy { get; set; }

        public DriverResult LapRecordQualyHolder { get; set; }

        public string LapRecordRace { get; set; }

        public DriverResult LapRecordRaceHolder { get; set; }
    }
}