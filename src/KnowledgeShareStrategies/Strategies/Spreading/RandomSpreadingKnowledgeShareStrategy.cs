using System;
using System.Linq;

namespace RelationalGit
{
    public class RandomSpreadingKnowledgeShareStrategy : BaseKnowledgeShareStrategy
    {
        private Random _random = new Random();

        public RandomSpreadingKnowledgeShareStrategy(string knowledgeSaveReviewerReplacementType) 
            : base(knowledgeSaveReviewerReplacementType)
        {
        }

        protected override DeveloperKnowledge[] GetCandidates(PullRequestContext pullRequestContext)
        {
            var availableDevelopers = pullRequestContext.AvailableDevelopers.Where(q => q.NormalizedName == pullRequestContext.PRSubmitterNormalizedName && (q.TotalCommits > 10 || q.TotalReviews > 10)).ToArray();
            var experiencedDevelopers = pullRequestContext.PRKnowledgeables;

            return availableDevelopers.Where(q => experiencedDevelopers.Any(e => e.DeveloperName == q.NormalizedName)).Select(q => new DeveloperKnowledge()
                {
                    DeveloperName = q.NormalizedName
                }).ToArray();
        }

        protected override DeveloperKnowledge[] SortActualReviewers(PullRequestContext pullRequestContext, DeveloperKnowledge[] actualReviewers)
        {
            return actualReviewers
                            .OrderBy(q => q.NumberOfReviews)
                            .ThenBy(q => q.NumberOfReviewedFiles)
                            .ThenBy(q => q.NumberOfCommits)
                            .ToArray();
        }

        protected override DeveloperKnowledge[] SortCandidates(PullRequestContext pullRequestContext, DeveloperKnowledge[] candidates)
        {
            return new DeveloperKnowledge[] { candidates[_random.Next(0, candidates.Length)] };
        }
    }
}
