using Microsoft.Extensions.Logging;
using RelationalGit.Simulation;

namespace RelationalGit.Recommendation
{
    public class RetentionRecRecommendationStrategy : ScoreBasedRecommendationStrategy
    {
        private int? _numberOfPeriodsForCalculatingProbabilityOfStay;

        public RetentionRecRecommendationStrategy(string knowledgeSaveReviewerReplacementType, 
            ILogger logger, 
            int? numberOfPeriodsForCalculatingProbabilityOfStay, 
            string pullRequestReviewerSelectionStrategy, 
            bool? addOnlyToUnsafePullrequests,
            string recommenderOption,
            bool changePast)
            : base(knowledgeSaveReviewerReplacementType, logger, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests,recommenderOption,changePast)
        {
            _numberOfPeriodsForCalculatingProbabilityOfStay = numberOfPeriodsForCalculatingProbabilityOfStay;
        }

        internal override double ComputeReviewerScore(PullRequestContext pullRequestContext, DeveloperKnowledge reviewer)
        {
            var probabilityOfStay = pullRequestContext.GetProbabilityOfStay(reviewer.DeveloperName, _numberOfPeriodsForCalculatingProbabilityOfStay.Value);
            var effort = pullRequestContext.GetEffort(reviewer.DeveloperName, _numberOfPeriodsForCalculatingProbabilityOfStay.Value);
            return effort * probabilityOfStay;
        }
    }
}
