using System;
using System.Collections.Generic;
using System.Text;

namespace RelationalGit
{
    public class DeveloperContribution
    {
        public long Id { get; set; }

        public string NormalizedName { get; set; }

        public long PeriodId { get; set; }

        public int TotalCommits {get ; set;}

        public bool IsCore { get; set; }

        public int TotalLines { get; set; }

        public double LinesPercentage { get; set; }     
    }
}
