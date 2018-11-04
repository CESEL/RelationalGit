using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace RelationalGit
{
    public class PullRequestContext
    {
        public string PRSubmitterNormalizedName { get; set; }

        internal Developer[] AvailableDevelopers;
        public string[] ActualReviewers { get; internal set; }
        public PullRequestFile[] PullRequestFiles { get; internal set; }
        public PullRequest PullRequest { get; internal set; }
        public KnowledgeDistributionMap KnowledgeMap { get; internal set; }
        public Dictionary<string, string> CanononicalPathMapper { get; internal set; }
        public Period Period { get; internal set; }
        public ReadOnlyDictionary<string, Developer> Developers { get; internal set; }
        public BlameSnapshot Blames { get; internal set; }
        public DeveloperKnowledge[] PRKnowledgeables { get; internal set; }

        public string WhoHasTheMostKnowlege()
        {
            var actualReviewers = ActualReviewers;

            for (var i = PRKnowledgeables.Length - 1; i >= 0; i--)
            {
                var isReviewer = actualReviewers.Any(q => q == PRKnowledgeables[i].DeveloperName);
                var isPrSubmitter = PRKnowledgeables[i].DeveloperName == PRSubmitterNormalizedName;
                var isAvailable = AvailableDevelopers.Any(q => q.NormalizedName == PRKnowledgeables[i].DeveloperName);

                if (!isReviewer && !isPrSubmitter && isAvailable)
                    return PRKnowledgeables[i].DeveloperName;
            }

            return null;
        }

        public string WhoHasTheLeastKnowledge()
        {
            var leastRank = int.MaxValue;
            var actualReviewers = ActualReviewers;

            for (var i = 0; i < actualReviewers.Length; i++)
            {
                var rank = Array.FindIndex(PRKnowledgeables, q => q.DeveloperName == actualReviewers[i]);

                if (rank != -1)
                {
                    //return actualReviewers[i];
                }

                if (rank!=-1 && rank < leastRank)
                    leastRank = rank;
            }

            if (leastRank == int.MaxValue)
            {
                return actualReviewers[0];
            }

            return PRKnowledgeables[leastRank].DeveloperName;
        }

        internal void SortPRKnowledgeables(Func<PullRequestContext, DeveloperKnowledge[]> sortPRKnowledgeables)
        {
            PRKnowledgeables = sortPRKnowledgeables(this);
        }

        public IEnumerable<DeveloperKnowledge> AvailablePRKnowledgeables()
        {
           return PRKnowledgeables.Where(q => AvailableDevelopers.Any(d => d.NormalizedName == q.DeveloperName));
        }
    }
}

