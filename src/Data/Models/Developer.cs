using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

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
        public Dictionary<long,DeveloperContribution> ContributionsPerPeriod { get; private set; }
        public long LastParticipationPeriodId => Math.Max(LastReviewPeriodId ?? 0,LastCommitPeriodId ?? 0);
        public long FirstParticipationPeriodId => Math.Min(FirstCommitPeriodId ?? int.MaxValue, FirstReviewPeriodId ?? int.MaxValue);
        internal void AddContributions(IEnumerable<DeveloperContribution> contributions)
        {
            if(ContributionsPerPeriod == null)
            {
                ContributionsPerPeriod = new Dictionary<long, DeveloperContribution>();
            }

            foreach (var contribution in contributions)
            {
                ContributionsPerPeriod[contribution.PeriodId] = contribution;
            }
        }

        public DeveloperContribution GetContributionsOfPeriod(long periodId)
        {
            return ContributionsPerPeriod.GetValueOrDefault(periodId);
        }
    }
}
