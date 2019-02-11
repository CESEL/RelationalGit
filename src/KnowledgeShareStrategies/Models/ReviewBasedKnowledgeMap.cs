using System;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;
using RelationalGit.KnowledgeShareStrategies.Models;

namespace RelationalGit
{
    public class ReviewBasedKnowledgeMap
    {
        private readonly Dictionary<string, Dictionary<string, DeveloperFileReveiewDetail>> _map = new Dictionary<string, Dictionary<string, DeveloperFileReveiewDetail>>();

        public Dictionary<string, DeveloperFileReveiewDetail> this[string filePath]
        {
            get
            {
                return _map.GetValueOrDefault(filePath);
            }
        }

        public IEnumerable<DeveloperFileReveiewDetail> Details => _map.Values.SelectMany(q => q.Values);

        public void Add(string filePath, IEnumerable<Developer> reviewersNamesOfPullRequest, PullRequest pullRequest, Period period)
        {
            if (filePath == null)
            {
                return;
            }

            if (!_map.ContainsKey(filePath))
            {
                _map[filePath] = new Dictionary<string, DeveloperFileReveiewDetail>();
            }

            foreach (var reviewer in reviewersNamesOfPullRequest)
            {
                AssignKnowledgeToReviewer(pullRequest, reviewer, period, filePath);
            }
        }

        internal IEnumerable<DeveloperFileReveiewDetail> GetReviewersOfPeriod(long periodId)
        {
            return _map.Values.SelectMany(q => q.Values.Where(c => c.Periods.Any(p => p.Id == periodId)));
        }

        private void AssignKnowledgeToReviewer(PullRequest pullRequest, Developer reviewer, Period period, string filePath)
        {
            var reviewerName = reviewer.NormalizedName;

            if (!_map[filePath].ContainsKey(reviewerName))
            {
                _map[filePath][reviewerName] = new DeveloperFileReveiewDetail()
                {
                    FilePath = filePath,
                    Developer = reviewer
                };
            }

            if (!_map[filePath][reviewerName].Periods.Any(q => q.Id == period.Id))
            {
                _map[filePath][reviewerName].Periods.Add(period);
            }

            _map[filePath][reviewerName].PullRequests.Add(pullRequest);
        }

        internal Dictionary<string, DeveloperFileReveiewDetail> GetReviewsOfFile(string filePath)
        {
            return _map.GetValueOrDefault(filePath);
        }
    }
}
