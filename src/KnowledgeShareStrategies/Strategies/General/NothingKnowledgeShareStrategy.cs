using Microsoft.Extensions.Logging;
using RelationalGit.KnowledgeShareStrategies.Models;
using System;

namespace RelationalGit
{
    public class NothingKnowledgeShareStrategy : KnowledgeShareStrategy
    {
        public NothingKnowledgeShareStrategy(string knowledgeSaveReviewerReplacementType, ILogger logger)
            : base(knowledgeSaveReviewerReplacementType, logger)
        {
        }

        protected override PullRequestRecommendationResult RecommendReviewers(PullRequestContext pullRequestContext)
        {
            return new PullRequestRecommendationResult(Array.Empty<DeveloperKnowledge>());
        }
    }
}
