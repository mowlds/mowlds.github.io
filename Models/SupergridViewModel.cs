using mowlds.github.io.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace mowlds.github.io.Models
{
    public class SupergridViewModel
    {
        public Season season { get; set; }
        public List<DriverResult> driverResults { get; set; }
        public List<Race> races { get; set; }

        public Driver driver { get; set; }
        public int totalPoints { get; set; }
        public int diffPoints { get; set; }
        
        public int highestPosition { get; set; }

        public int currentTablePosition { get; set; }
       
    }
}