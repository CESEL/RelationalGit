using Microsoft.Extensions.Logging;
using RelationalGit.Simulation;
using System;

namespace RelationalGit.Recommendation
{
    public class RealityRecommendationStrategy : RecommendationStrategy
    {
        public RealityRecommendationStrategy(string knowledgeSaveReviewerReplacementType, ILogger logger, bool changePast)
            : base(knowledgeSaveReviewerReplacementType, logger, changePast)
        {
        }

        protected override Simulation.PullRequestRecommendationResult RecommendReviewers(PullRequestContext pullRequestContext)
        {
            return new Simulation.PullRequestRecommendationResult(pullRequestContext.ActualReviewers, Array.Empty<DeveloperKnowledge>(),null,null);
        }
    }
}
