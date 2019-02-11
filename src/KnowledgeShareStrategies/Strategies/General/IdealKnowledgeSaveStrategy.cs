using System.Linq;
using RelationalGit.KnowledgeShareStrategies.Models;

namespace RelationalGit
{
    public class IdealKnowledgeShareStrategy : KnowledgeShareStrategy
    {
        public IdealKnowledgeShareStrategy(string knowledgeSaveReviewerReplacementType)
            : base(knowledgeSaveReviewerReplacementType)
        {
        }

        protected override PullRequestRecommendationResult RecommendReviewers(PullRequestContext pullRequestContext)
        {
            var oldestDevelopers = pullRequestContext.Developers.Values.Where(q => q.FirstCommitPeriodId <= pullRequestContext.PullRequestPeriod.Id);

            var longtermStayedDeveloper = oldestDevelopers.OrderBy(q => q.LastCommitPeriodId - q.FirstCommitPeriodId).Last();

            return new PullRequestRecommendationResult(new string[] { longtermStayedDeveloper.NormalizedName});
        }
    }
}
