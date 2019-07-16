using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RelationalGit.Data;

namespace RelationalGit.Simulation
{
    public class PullRequestContext
    {
        private static Dictionary<long, (int TotalReviews, int TotalCommits)> _totalContributionsInPeriodDic = new Dictionary<long, (int TotalReviews, int TotalCommits)>();

        public string PRSubmitterNormalizedName { get; set; }

        public string SelectedReviewersType { get; set; }

        public Developer[] AvailableDevelopers;

        private bool? _isSafe;

        private static Dictionary<string, (int TotalReviews, int TotalCommits)> _contributionsDic = new Dictionary<string, (int TotalReviews, int TotalCommits)>();

        private static CommitComparer _commitComparer = new CommitComparer();

        private static PullRequestComparer _pullRequestComparer = new PullRequestComparer();

        public DeveloperKnowledge[] ActualReviewers { get;   set; }

        public PullRequestFile[] PullRequestFiles { get;   set; }

        public PullRequest PullRequest { get;   set; }

        public KnowledgeDistributionMap KnowledgeMap { get;   set; }

        public Dictionary<string, string> CanononicalPathMapper { get;   set; }

        public Period PullRequestPeriod { get;   set; }

        public ReadOnlyDictionary<string, Developer> Developers { get;   set; }

        public BlameSnapshot Blames { get;   set; }

        public DeveloperKnowledge[] PullRequestKnowledgeables { get;   set; }

        public Dictionary<long, Period> Periods { get;   set; }

        public HashSet<string> Hoarders { get;   set; } = null;

        public bool PullRequestFilesAreSafe
        {
            get
            {

                if (_isSafe == null)
                {
                    FindHoarders();
                }

                return _isSafe.Value;
            }
        }

        public bool IsHoarder(string normalizedDeveloperName)
        {
            if (Hoarders == null)
            {
                FindHoarders();
            }

            return Hoarders.Contains(normalizedDeveloperName);
        }

        private void FindHoarders()
        {
            Hoarders = new HashSet<string>();
            var availableDevelopersOfPeriod = AvailableDevelopers.Select(q => q.NormalizedName).ToHashSet();
            var blameSnapshot = KnowledgeMap.BlameBasedKnowledgeMap.GetSnapshopOfPeriod(PullRequestPeriod.Id);

            foreach (var pullRequestFile in PullRequestFiles)
            {
                var canonicalPath = CanononicalPathMapper.GetValueOrDefault(pullRequestFile.FileName);

                if (canonicalPath == null)
                {
                    continue;
                }

                var blames = blameSnapshot[canonicalPath];

                var committers = blames?.Where(q => q.Value.OwnedPercentage > 0)
                    ?.Select(q => q.Value.NormalizedDeveloperName) ?? Array.Empty<string>();

                var reviewers = KnowledgeMap.ReviewBasedKnowledgeMap[canonicalPath]?.Where(q => q.Value.Periods.Any(p => p.Id <= PullRequestPeriod.Id))
                    ?.Select(q => q.Value.Developer.NormalizedName) ?? Array.Empty<string>();

                var availableContributors = committers.Union(reviewers).Where(q => availableDevelopersOfPeriod.Contains(q)).ToArray();

                if (availableContributors.Length < 2)
                {
                    _isSafe = false;

                    if (availableContributors.Length == 1)
                    {
                        Hoarders.Add(availableContributors[0]);
                    }
                }
            }

            if (!_isSafe.HasValue)
            {
                _isSafe = true;
            }
        }

        public double GetEffort(string developer, int numberOfPeriodsForCalculatingProbabilityOfStay)
        {
            var lastYear = PullRequest.CreatedAtDateTime.Value.Subtract(TimeSpan.FromDays(365));

            var totalContribution = GetTotalContributionsBestweenPeriods(lastYear, PullRequest.CreatedAtDateTime.Value);
            var developerTotalContribution = GetDeveloperTotalContributionsBestweenPeriods(lastYear, PullRequest.CreatedAtDateTime.Value, developer);

            return ((developerTotalContribution.TotalReviews) + developerTotalContribution.TotalCommits)
                / (double)((totalContribution.TotalReviews) + totalContribution.TotalCommits);
        }
        /*
        public double GetEffort(string developer, int numberOfPeriodsForCalculatingProbabilityOfStay)
        {
            var currentPeriod = PullRequestPeriod;
            var lastYearPeriod = Periods.GetValueOrDefault(currentPeriod.Id - numberOfPeriodsForCalculatingProbabilityOfStay + 1);

            var totalContribution = GetTotalContributionsBestweenPeriods(lastYearPeriod,currentPeriod);
            var developerTotalContribution = GetDeveloperTotalContributionsBestweenPeriods(lastYearPeriod, currentPeriod, developer);

            return ((developerTotalContribution.TotalReviews) + developerTotalContribution.TotalCommits)
                / (double) ((totalContribution.TotalReviews) + totalContribution.TotalCommits);
        }*/

        /*private (int TotalReviews, int TotalCommits) GetDeveloperTotalContributionsBestweenPeriods(Period lastYearPeriod, Period currentPeriod, string developer)
        {
            var key = (lastYearPeriod?.Id ?? 1) + "-" + currentPeriod.Id + "-" + developer;

            if (!_contributionsDic.ContainsKey(key))
            {
                var numberOfReviews = 0;
                var numberOfCommits = 0;

                for (long periodId = lastYearPeriod?.Id ?? 1; periodId <= currentPeriod.Id; periodId++)
                {
                    var contribution = Developers[developer].ContributionsPerPeriod.GetValueOrDefault(periodId);

                    numberOfReviews += contribution?.TotalReviews ?? 0;
                    numberOfCommits += contribution?.TotalCommits ?? 0;
                }

                _contributionsDic[key] = (numberOfReviews, numberOfCommits);

            }

            return _contributionsDic[key];
        }*/

        /*private (int TotalReviews, int TotalCommits) GetTotalContributionsBestweenPeriods(Period lastYearPeriod, Period currentPeriod)
        {
            var key = (lastYearPeriod?.Id ?? 1) + "-" + currentPeriod.Id;

            if (!_contributionsDic.ContainsKey(key))
            {
                var totalCommitsSoFar = 0;
                var totalReviewsSoFar = 0;

                for (long periodId = lastYearPeriod?.Id ?? 1; periodId <= currentPeriod.Id; periodId++)
                {
                    var totalContributionOfPeriod = GetTotalContributionsOfPeriod(periodId);

                    totalCommitsSoFar += totalContributionOfPeriod.TotalCommits;
                    totalReviewsSoFar += totalContributionOfPeriod.TotalReviews;
                }

                _contributionsDic[key] = (totalReviewsSoFar, totalCommitsSoFar);
            }

            return _contributionsDic[key];
        }*/

        private (int TotalReviews, int TotalCommits) GetDeveloperTotalContributionsBestweenPeriods(DateTime from, DateTime to, string developer)
        {
            var totalCommits = 0;
            var commits = KnowledgeMap.CommitBasedKnowledgeMap.GetDeveloperCommits(developer);
            var baselineCommit = new Commit() { AuthorDateTime = from };
            var index = commits.BinarySearch(baselineCommit,_commitComparer);

            if (index < 0)
                index = ~index;
            totalCommits += commits.Count - index;

            var totalReviews = 0;
            var reviews = KnowledgeMap.ReviewBasedKnowledgeMap.GetDeveloperReviews(developer);
            var baselinePullRequest = new PullRequest() { CreatedAtDateTime = from };

            index = reviews.BinarySearch(baselinePullRequest,_pullRequestComparer);

            if (index < 0)
                index = ~index;
            totalReviews += reviews.Count - index;

            return (totalReviews, totalCommits);
        }

        private (int TotalReviews, int TotalCommits) GetTotalContributionsBestweenPeriods(DateTime from, DateTime to)
        {
            var totalCommits = 0;
            var committers = KnowledgeMap.CommitBasedKnowledgeMap.GetCommitters();
            var baselineCommit = new Commit() { AuthorDateTime = from };

            foreach (var committer in committers)
            {
                var index = committer.Value.BinarySearch(baselineCommit,_commitComparer);

                if (index < 0)
                    index = ~index;
                totalCommits += committer.Value.Count - index;
            }

            var totalReviews = 0;
            var reviewers = KnowledgeMap.ReviewBasedKnowledgeMap.GetReviewers();
            var baselinePullRequest = new PullRequest() { CreatedAtDateTime = from };

            foreach (var reviewer in reviewers)
            {
                var index = reviewer.Value.BinarySearch(baselinePullRequest,_pullRequestComparer);

                if (index < 0)
                    index = ~index;
                totalReviews += reviewer.Value.Count - index;
            }       

            return (totalReviews, totalCommits);
        }

        public double GetProbabilityOfStay(string reviewer, int numberOfPeriodsForCalculatingProbabilityOfStay)
        {
            var currentPeriodId = PullRequestPeriod.Id;
            var lastYearPeriodId = currentPeriodId - numberOfPeriodsForCalculatingProbabilityOfStay + 1;

            var numberOfContributedPeriodsSoFar = Developers[reviewer].AllCommitsPeriodsId.Where(q => q <= currentPeriodId && q >= lastYearPeriodId)
                .Union(Developers[reviewer].AllReviewsPeriodsId.Where(q => q <= currentPeriodId && q >= lastYearPeriodId)).Count();

            return numberOfContributedPeriodsSoFar / (double) numberOfPeriodsForCalculatingProbabilityOfStay;
        }

        private (int TotalReviews, int TotalCommits) GetTotalContributionsOfPeriod(long periodId)
        {
            if (!_totalContributionsInPeriodDic.ContainsKey(periodId))
            {
                var totalCommits = Developers.Sum(q => q.Value.ContributionsPerPeriod.GetValueOrDefault(periodId)?.TotalCommits ?? 0);
                var totalReviews = Developers.Sum(q => q.Value.ContributionsPerPeriod.GetValueOrDefault(periodId)?.TotalReviews ?? 0);

                _totalContributionsInPeriodDic[periodId] = (totalReviews, totalCommits);
            }

            return _totalContributionsInPeriodDic[periodId];
        }
    }

    public class CommitComparer : IComparer<Commit>
    {
        public int Compare(Commit x, Commit y)
        {
            return x.AuthorDateTime.CompareTo(y.AuthorDateTime);
        }
    }

    public class PullRequestComparer : IComparer<PullRequest>
    {
        public int Compare(PullRequest x, PullRequest y)
        {
            return x.CreatedAtDateTime.Value.CompareTo(y.CreatedAtDateTime);
        }
    }
}
