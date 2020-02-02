using Microsoft.Extensions.Logging;
using RelationalGit.Simulation;
using System.Linq;

namespace RelationalGit.Recommendation
{
    public class ContributionRecRecommendationStrategy : ScoreBasedRecommendationStrategy
    {
        public ContributionRecRecommendationStrategy(string knowledgeSaveReviewerReplacementType, ILogger logger, string pullRequestReviewerSelectionStrategy, bool? addOnlyToUnsafePullrequests, string recommenderOption, bool changePast)
            : base(knowledgeSaveReviewerReplacementType, logger, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests,recommenderOption, changePast)
        {
        }

        internal override double ComputeReviewerScore(PullRequestContext pullRequestContext, DeveloperKnowledge reviewer)
        {
            var totalCommits = pullRequestContext.PullRequestKnowledgeables.Sum(q=>q.NumberOfCommits);
            var totalReviews = pullRequestContext.PullRequestKnowledgeables.Sum(q => q.NumberOfReviews);

            if (totalCommits + totalReviews ==0)
            {
                return 0;
            }

            return (reviewer.NumberOfCommits + reviewer.NumberOfCommits) / (double)(totalCommits + totalReviews);
        }
    }
}
