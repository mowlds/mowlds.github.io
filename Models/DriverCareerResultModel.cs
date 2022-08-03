using mowlds.github.io.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace mowlds.github.io.Models
{
    public class DriverCareerResultModel : SupergridViewModel
    {
        public int finalPosition { get; set; }

        public List<Track> allTracks { get; set; }
    }
}