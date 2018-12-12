using System.Linq;
using RelationalGit.KnowledgeShareStrategies.Models;

namespace RelationalGit
{
    public class RealisticIdealKnowledgeShareStrategy : KnowledgeShareStrategy
    {
        public RealisticIdealKnowledgeShareStrategy(string knowledgeSaveReviewerReplacementType)
            : base(knowledgeSaveReviewerReplacementType)
        {
        }

        protected override PullRequestRecommendationResult RecommendReviewers(PullRequestContext pullRequestContext)
        {
            if (pullRequestContext.ActualReviewers.Count() == 0)
            {
                return new PullRequestRecommendationResult(new string[0]);
            }

            var oldestDevelopers = pullRequestContext.Developers.Values.Where(q => q.FirstCommitPeriodId <= pullRequestContext.Period.Id);

            var longtermStayedDeveloper = oldestDevelopers.OrderBy(q => q.LastCommitPeriodId - q.FirstCommitPeriodId).Last();

            return new PullRequestRecommendationResult(new string[] { longtermStayedDeveloper.NormalizedName });
        }
    }
}
