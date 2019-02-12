using Microsoft.Extensions.Logging;
using System.Linq;

namespace RelationalGit
{
    public class CommitBasedKnowledgeShareStrategy : BaseKnowledgeShareStrategy
    {
        public CommitBasedKnowledgeShareStrategy(string knowledgeSaveReviewerReplacementType, ILogger logger)
            : base(knowledgeSaveReviewerReplacementType, logger)
        {
        }

        protected override DeveloperKnowledge[] SortCandidates(PullRequestContext pullRequestContext, DeveloperKnowledge[] candidates)
        {
            return candidates
            .OrderBy(q => q.NumberOfCommits)
            .ThenBy(q => q.NumberOfCommittedFiles)
            .ThenBy(q => q.NumberOfAuthoredLines)
            .ToArray();
        }
    }
}