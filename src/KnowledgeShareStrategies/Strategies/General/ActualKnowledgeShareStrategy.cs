using RelationalGit.KnowledgeShareStrategies.Models;

namespace RelationalGit
{
    public class ActualKnowledgeShareStrategy : KnowledgeShareStrategy
    {
        public ActualKnowledgeShareStrategy(string knowledgeSaveReviewerReplacementType)
            : base(knowledgeSaveReviewerReplacementType)
        {
        }

        protected override PullRequestRecommendationResult RecommendReviewers(PullRequestContext pullRequestContext)
        {
            return new PullRequestRecommendationResult(pullRequestContext.ActualReviewers, null);
        }
    }
}
