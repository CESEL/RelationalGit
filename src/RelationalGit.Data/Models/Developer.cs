using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace RelationalGit.Data
{
    public class Developer
    {
        private int[] _allCommitsPeriodsId;
        private int[] _allReviewsPeriodsId;

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

        public DateTime? FirstCommitDateTime { get; set; }

        public DateTime? LastCommitDateTime { get; set; }

        public DateTime? FirstReviewDateTime { get; set; }

        public DateTime? LastReviewDateTime { get; set; }

        public DateTime FirstParticipationDateTime
        {
            get
            {
                if (FirstCommitDateTime.HasValue && !FirstReviewDateTime.HasValue)
                {
                    return FirstCommitDateTime.Value;
                }

                if (!FirstCommitDateTime.HasValue && FirstReviewDateTime.HasValue)
                {
                    return FirstReviewDateTime.Value;
                }

                return FirstCommitDateTime.Value < FirstReviewDateTime.Value
                    ? FirstCommitDateTime.Value : FirstReviewDateTime.Value;
            }
        }


        public DateTime LastParticipationDateTime
        {
            get
            {
                if (LastCommitDateTime.HasValue && !LastReviewDateTime.HasValue)
                {
                    return LastCommitDateTime.Value;
                }

                if (!LastCommitDateTime.HasValue && LastReviewDateTime.HasValue)
                {
                    return LastReviewDateTime.Value;
                }

                return LastCommitDateTime.Value < LastReviewDateTime.Value
                    ? LastReviewDateTime.Value : LastCommitDateTime.Value;
            }
        }

        public int[] AllCommitsPeriodsId
        {
            get
            {
                if (_allCommitsPeriodsId == null)
                {
                    _allCommitsPeriodsId = string.IsNullOrEmpty(AllCommitPeriods) ? Array.Empty<int>() : AllCommitPeriods?.Split(',').Select(q => int.Parse(q)).ToArray();
                }

                return _allCommitsPeriodsId;
            }
        }

        public int[] AllReviewsPeriodsId
        {
            get
            {
                if (_allReviewsPeriodsId == null)
                {
                    _allReviewsPeriodsId = string.IsNullOrEmpty(AllReviewPeriods) ? Array.Empty<int>() : AllReviewPeriods.Split(',').Select(q => int.Parse(q)).ToArray();
                }

                return _allReviewsPeriodsId;
            }
        }
        
        [NotMapped]
        public Dictionary<long, DeveloperContribution> ContributionsPerPeriod { get; private set; }

        public long LastParticipationPeriodId => Math.Max(LastReviewPeriodId ?? 0, LastCommitPeriodId ?? 0);

        public long FirstParticipationPeriodId => Math.Min(FirstCommitPeriodId ?? int.MaxValue, FirstReviewPeriodId ?? int.MaxValue);

        public void AddContributions(IEnumerable<DeveloperContribution> contributions)
        {
            if (ContributionsPerPeriod == null)
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
