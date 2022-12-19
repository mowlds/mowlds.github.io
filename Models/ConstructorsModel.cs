using mowlds.github.io.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace mowlds.github.io.Models
{
    public class ConstructorsModel
    {
        public Team team { get; set; }
        public Driver driver1 { get; set; }
        public Driver driver2 { get; set; }
        public int totalPoints { get; set; }
        public int totalWins { get; set; }

        public int position { get; set; }
    }
}