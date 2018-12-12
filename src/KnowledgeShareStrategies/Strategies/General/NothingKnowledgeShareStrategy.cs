using RelationalGit.KnowledgeShareStrategies.Models;

namespace RelationalGit
{
    public class NothingKnowledgeShareStrategy : KnowledgeShareStrategy
    {

        public NothingKnowledgeShareStrategy(string knowledgeSaveReviewerReplacementType)
            : base(knowledgeSaveReviewerReplacementType)
        {
        }

        protected override PullRequestRecommendationResult RecommendReviewers(PullRequestContext pullRequestContext)
        {
            return new PullRequestRecommendationResult(new string[0]);
        }
    }
}
