using Microsoft.Extensions.Logging;
using RelationalGit.Simulation;
using System.Linq;

namespace RelationalGit.Recommendation
{
    public class AuthorshipRecRecommendationStrategy : ScoreBasedRecommendationStrategy
    {
        public AuthorshipRecRecommendationStrategy(string knowledgeSaveReviewerReplacementType, ILogger logger, string pullRequestReviewerSelectionStrategy, bool? addOnlyToUnsafePullrequests, string recommenderOption, bool changePast)
            : base(knowledgeSaveReviewerReplacementType, logger, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests,recommenderOption, changePast)
        {
        }

        internal override double ComputeReviewerScore(PullRequestContext pullRequestContext, DeveloperKnowledge reviewer)
        {
            var totalCommits = pullRequestContext.PullRequestKnowledgeables.Sum(q=>q.NumberOfCommits);

            if(totalCommits==0)
            {
                return 0;
            }

            return reviewer.NumberOfCommits / (double)totalCommits;
        }
    }
}
