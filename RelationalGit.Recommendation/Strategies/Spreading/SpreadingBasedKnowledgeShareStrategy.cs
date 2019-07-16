using Microsoft.Extensions.Logging;
using RelationalGit.Simulation;

namespace RelationalGit.Recommendation
{
    public class SpreadingBasedKnowledgeShareStrategy : ScoreBasedSpreadingKnowledgeShareStrategy
    {
        public SpreadingBasedKnowledgeShareStrategy(string knowledgeSaveReviewerReplacementType, ILogger logger, string pullRequestReviewerSelectionStrategy, bool? addOnlyToUnsafePullrequests,string recommenderOption, bool changePast)
            : base(knowledgeSaveReviewerReplacementType, logger, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests,recommenderOption, changePast)
        {
        }

        internal override double ComputeReviewerScore(PullRequestContext pullRequestContext, DeveloperKnowledge reviewer)
        {
            var specializedKnowledge = reviewer.NumberOfTouchedFiles / (double)pullRequestContext.PullRequestFiles.Length;
            return 1 - specializedKnowledge;
        }
    }
}
