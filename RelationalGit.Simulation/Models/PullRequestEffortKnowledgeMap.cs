using System;
using System.Collections.Generic;
using System.Linq;
using RelationalGit.Data;

namespace RelationalGit.Simulation
{
    /// <summary>
    /// Based on Bird's approach
    /// </summary>
    public class PullRequestEffortKnowledgeMap
    {
        private readonly Dictionary<string, Dictionary<string, (int TotalComments, HashSet<string> Workdays)>> _map = new Dictionary<string, Dictionary<string, (int TotalComments, HashSet<string> Workdays)>>();

        public void Add(string filePath,  Developer developer, DateTime reviewDate)
        {
            if (filePath == null || developer == null)
            {
                return;
            }

            if (!_map.ContainsKey(filePath))
            {
                _map[filePath] = new Dictionary<string, (int TotalComments, HashSet<string> Workdays)>();
            }

            if (!_map[filePath].ContainsKey(developer.NormalizedName))
            {
                _map[filePath][developer.NormalizedName] = (0, new HashSet<string>());
            }

            var (totalComments, workdays) = _map[filePath][developer.NormalizedName];

            totalComments = +1;
            workdays.Add(reviewDate.ToShortDateString());

            _map[filePath][developer.NormalizedName] = (totalComments, workdays);
        }

        public (int TotalComments, int TotalWorkDays, DateTime? RecentWorkDay) GetFileExpertise(string filePath)
        {
            var reviewersExpertise = _map.GetValueOrDefault(filePath);

            if (reviewersExpertise == null)
            {
                return (0, 0, null);
            }

            var totalComments = reviewersExpertise.Sum(q => q.Value.TotalComments);
            var totalWorkDays = reviewersExpertise.SelectMany(q => q.Value.Workdays.ToArray()).Distinct().Count();
            var recentWorkday = reviewersExpertise.Max(q => q.Value.Workdays.Max());

            return (totalComments, totalWorkDays, DateTime.Parse(recentWorkday));
        }

        public (int TotalComments, int TotalWorkDays, DateTime? RecentWorkDay) GetReviewerExpertise(string filePath, string reviewerName)
        {
            var reviewerExpertise = _map.GetValueOrDefault(filePath)?.GetValueOrDefault(reviewerName);

            if (reviewerExpertise == null || reviewerExpertise == (0, null))
            {
                return (0, 0, null);
            }

            var totalComments = reviewerExpertise.Value.TotalComments;
            var totalWorkDays = reviewerExpertise.Value.Workdays.Count();
            var recentWorkday = reviewerExpertise.Value.Workdays.Max();

            return (totalComments, totalWorkDays, DateTime.Parse(recentWorkday));
        }
    }
}
