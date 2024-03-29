//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace mowlds.github.io.DAL
{
    using System;
    using System.Collections.Generic;
    
    public partial class Race
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Race()
        {
            this.DriverResult = new HashSet<DriverResult>();
            this.RaceLaps = new HashSet<RaceLaps>();
        }
    
        public int ID { get; set; }
        public int Season { get; set; }
        public int Track { get; set; }
        public int RaceNumber { get; set; }
        public string RaceName { get; set; }
        public Nullable<System.DateTime> RaceDate { get; set; }
        public Nullable<int> Laps { get; set; }
        public string Review { get; set; }
        public string Highlights { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DriverResult> DriverResult { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RaceLaps> RaceLaps { get; set; }
        public virtual Season Season1 { get; set; }
        public virtual Track Track1 { get; set; }
    }
}
