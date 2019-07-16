using Microsoft.Extensions.Logging;
using RelationalGit.Data;
using RelationalGit.Simulation;
using System;

namespace RelationalGit.Recommendation
{
    public class ActualKnowledgeShareStrategy : KnowledgeShareStrategy
    {
        public ActualKnowledgeShareStrategy(string knowledgeSaveReviewerReplacementType, ILogger logger, bool changePast)
            : base(knowledgeSaveReviewerReplacementType, logger, changePast)
        {
        }

        protected override Simulation.PullRequestRecommendationResult RecommendReviewers(PullRequestContext pullRequestContext)
        {
            return new Simulation.PullRequestRecommendationResult(pullRequestContext.ActualReviewers, Array.Empty<DeveloperKnowledge>());
        }
    }
}
