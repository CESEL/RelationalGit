using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace RelationalGit
{
    public class Developer
    {
        public long Id { get; set; }
        public string NormalizedName { get; set; }
        public long? FirstCommitPeriodId { get; set; }
        public long? LastCommitPeriodId { get; set; }
        public string AllCommitPeriods { get; set; }
        public long? FirstReviewPeriodId { get; set; }
        public long? LastReviewPeriodId { get; set; }
        public string AllReviewPeriods { get; set; }
        public int TotalCommits { get; set; }
        public int TotalReviews { get; set; }
        public int[] AllCommitsPeriodsId => AllCommitPeriods.Split(',').Select(q => int.Parse(q)).ToArray();
        public int[] AllReviewsPeriodsId => AllReviewPeriods.Split(',').Select(q => int.Parse(q)).ToArray();

        [NotMapped]
        public List<DeveloperContribution> Contributions { get; private set; }
        public long LastParticipationPeriodId => Math.Max(LastReviewPeriodId??0,LastCommitPeriodId??0);
        public long FirstParticipationPeriodId => Math.Min(FirstCommitPeriodId ?? 0, FirstReviewPeriodId ?? 0);
        internal void AddContributions(IEnumerable<DeveloperContribution> contributions)
        {
            if(Contributions is null)
                Contributions= new List<DeveloperContribution>();

            Contributions.AddRange(contributions);
        }
    }
}
