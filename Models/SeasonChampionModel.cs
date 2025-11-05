using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace mowlds.github.io.Models
{
    public class SeasonChampionModel : DAL.Season
    {
        public string Champion { get; set; }

        public int NumberOfTitles { get; set; }
    }
}