using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelationalGit
{
    public class Developer
    {
        public long Id { get; set; }
        public string NormalizedName { get; set; }
        public long? FirstPeriodId { get; set; }
        public long? LastPeriodId {get ; set;}
        public string AllPeriods { get; set; }
        public int TotalCommits { get; set; }     
        public int[] AllPeriodsId => AllPeriods.Split(',').Select(q=>int.Parse(q)).ToArray();
    }
}
