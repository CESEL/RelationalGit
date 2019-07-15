using Microsoft.Extensions.Logging;
using RelationalGit.KnowledgeShareStrategies.Models;
using System;

namespace RelationalGit
{
    public class ActualKnowledgeShareStrategy : KnowledgeShareStrategy
    {
        public ActualKnowledgeShareStrategy(string knowledgeSaveReviewerReplacementType, ILogger logger)
            : base(knowledgeSaveReviewerReplacementType, logger)
        {
        }

        protected override PullRequestRecommendationResult RecommendReviewers(PullRequestContext pullRequestContext)
        {
            return new PullRequestRecommendationResult(pullRequestContext.ActualReviewers, Array.Empty<DeveloperKnowledge>());
        }
    }
}
