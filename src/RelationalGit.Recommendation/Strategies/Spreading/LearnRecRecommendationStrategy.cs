using Microsoft.Extensions.Logging;
using RelationalGit.Simulation;
using System.Linq;

namespace RelationalGit.Recommendation
{
    public class LearnRecRecommendationStrategy : ScoreBasedRecommendationStrategy
    {
        public LearnRecRecommendationStrategy(string knowledgeSaveReviewerReplacementType, ILogger logger, string pullRequestReviewerSelectionStrategy, bool? addOnlyToUnsafePullrequests,string recommenderOption, bool changePast)
            : base(knowledgeSaveReviewerReplacementType, logger, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests,recommenderOption, changePast)
        {
        }

        internal override double ComputeReviewerScore(PullRequestContext pullRequestContext, DeveloperKnowledge reviewer)
        {
            var prFiles = pullRequestContext.PullRequestFiles.Select(q => pullRequestContext.CanononicalPathMapper[q.FileName])
                .Where(q => q != null).ToArray();

            var reviewedFiles = reviewer.GetTouchedFiles().Where(q=>prFiles.Contains(q));

            var specializedKnowledge = reviewedFiles.Count() / (double)pullRequestContext.PullRequestFiles.Length;

            return 1 - specializedKnowledge;
        }
    }
}
