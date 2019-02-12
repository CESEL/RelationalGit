using System.Linq;
using Microsoft.Extensions.Logging;
using RelationalGit.KnowledgeShareStrategies.Models;

namespace RelationalGit
{
    public class IdealKnowledgeShareStrategy : KnowledgeShareStrategy
    {
        public IdealKnowledgeShareStrategy(string knowledgeSaveReviewerReplacementType, ILogger logger)
            : base(knowledgeSaveReviewerReplacementType, logger)
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
