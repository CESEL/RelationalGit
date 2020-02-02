using Microsoft.Extensions.Logging;
using RelationalGit.Simulation;
using System.Linq;

namespace RelationalGit.Recommendation
{
    public class LineRecRecommendationStrategy : ScoreBasedRecommendationStrategy
    {
        public LineRecRecommendationStrategy(string knowledgeSaveReviewerReplacementType, ILogger logger, string pullRequestReviewerSelectionStrategy, bool? addOnlyToUnsafePullrequests, string recommenderOption, bool changePast)
            : base(knowledgeSaveReviewerReplacementType, logger, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests,recommenderOption, changePast)
        {
        }

        internal override double ComputeReviewerScore(PullRequestContext pullRequestContext, DeveloperKnowledge reviewer)
        {
            var totalLines = pullRequestContext.PullRequestKnowledgeables.Sum(q => q.NumberOfAuthoredLines);

            if (totalLines == 0)
                return 0;

            return reviewer.NumberOfAuthoredLines / (double)totalLines;
        }
    }
}
