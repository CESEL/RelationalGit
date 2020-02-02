using Microsoft.Extensions.Logging;
using RelationalGit.Simulation;
using System.Linq;

namespace RelationalGit.Recommendation
{
    public class RevOwnRecRecommendationStrategy : ScoreBasedRecommendationStrategy
    {
        public RevOwnRecRecommendationStrategy(string knowledgeSaveReviewerReplacementType, ILogger logger, string pullRequestReviewerSelectionStrategy, bool? addOnlyToUnsafePullrequests,string recommenderOption, bool changePast)
            : base(knowledgeSaveReviewerReplacementType, logger, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests,recommenderOption, changePast)
        {
        }

        internal override double ComputeReviewerScore(PullRequestContext pullRequestContext, DeveloperKnowledge reviewer)
        {
            var totalReviews = pullRequestContext.PullRequestKnowledgeables.Sum(q => q.NumberOfReviews);

            if(totalReviews == 0)
            {
                return 0;
            }

            return reviewer.NumberOfReviews / (double)totalReviews;
        }
    }
}
