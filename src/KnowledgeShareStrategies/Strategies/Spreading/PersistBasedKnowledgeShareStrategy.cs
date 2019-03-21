using Microsoft.Extensions.Logging;

namespace RelationalGit.KnowledgeShareStrategies.Strategies.Spreading
{
    public class PersistBasedKnowledgeShareStrategy : ScoreBasedSpreadingKnowledgeShareStrategy
    {
        private int? _numberOfPeriodsForCalculatingProbabilityOfStay;

        public PersistBasedKnowledgeShareStrategy(string knowledgeSaveReviewerReplacementType, ILogger logger, int? numberOfPeriodsForCalculatingProbabilityOfStay, string pullRequestReviewerSelectionStrategy, bool? addOnlyToUnsafePullrequests)
            : base(knowledgeSaveReviewerReplacementType, logger, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests)
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
