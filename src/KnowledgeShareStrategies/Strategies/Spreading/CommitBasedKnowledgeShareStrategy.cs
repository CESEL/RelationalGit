using Microsoft.Extensions.Logging;
using System.Linq;

namespace RelationalGit.KnowledgeShareStrategies.Strategies.Spreading
{
    public class CommitBasedKnowledgeShareStrategy : ScoreBasedSpreadingKnowledgeShareStrategy
    {
        public CommitBasedKnowledgeShareStrategy(string knowledgeSaveReviewerReplacementType, ILogger logger, string pullRequestReviewerSelectionStrategy, bool? addOnlyToUnsafePullrequests)
            : base(knowledgeSaveReviewerReplacementType, logger, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests)
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
