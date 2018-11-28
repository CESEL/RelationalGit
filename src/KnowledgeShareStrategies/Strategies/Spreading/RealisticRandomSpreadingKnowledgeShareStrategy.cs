using RelationalGit.KnowledgeShareStrategies.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RelationalGit
{
    public class RealisticRandomSpreadingKnowledgeShareStrategy : KnowledgeShareStrategy
    {
        private Random _random = new Random();

        public RealisticRandomSpreadingKnowledgeShareStrategy(string knowledgeSaveReviewerReplacementType) : base(knowledgeSaveReviewerReplacementType)
        { }

        protected override PullRequestRecommendationResult RecommendReviewers(PullRequestContext pullRequestContext)
        {
            /*
            if (pullRequestContext.ActualReviewers.Count() == 0)
                return pullRequestContext.ActualReviewers;

            var availableDevelopers = pullRequestContext.AvailableDevelopers.Where(q => q.TotalCommits + q.TotalReviews > 10).Select(q => q.NormalizedName).ToArray();

            pullRequestContext.PRKnowledgeables = pullRequestContext.PRKnowledgeables
                .OrderBy(q => q.NumberOfTouchedFiles)
                .OrderBy(q => q.NumberOfReviews + q.NumberOfCommits)
                .ToArray();

            var experiencedDevelopers = pullRequestContext.PRKnowledgeables.Select(q => q.DeveloperName);
            var nonexperiencedDevelopers = availableDevelopers.Except(experiencedDevelopers).ToArray();

            if (nonexperiencedDevelopers.Length == 0)
                return pullRequestContext.ActualReviewers;

            var randomDeveloper = nonexperiencedDevelopers[_random.Next(0, nonexperiencedDevelopers.Length)];

            var leastKnowledgeable = pullRequestContext.WhoHasTheLeastKnowledge();

            var leastIndex = Array.FindIndex(pullRequestContext.ActualReviewers, q => q == leastKnowledgeable);

            pullRequestContext.ActualReviewers[leastIndex] = randomDeveloper;

            return pullRequestContext.ActualReviewers;*/

            return null;
        }

    }
}
