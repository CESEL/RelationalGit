using Microsoft.Extensions.Logging;
using System.Linq;

namespace RelationalGit.KnowledgeShareStrategies.Strategies.Spreading
{
    public class BlameBasedKnowledgeShareStrategy : ScoreBasedSpreadingKnowledgeShareStrategy
    {
        public BlameBasedKnowledgeShareStrategy(string knowledgeSaveReviewerReplacementType, ILogger logger, string pullRequestReviewerSelectionStrategy, bool? addOnlyToUnsafePullrequests, string recommenderOption, bool changePast)
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
