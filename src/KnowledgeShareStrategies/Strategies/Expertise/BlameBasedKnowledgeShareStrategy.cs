using Microsoft.Extensions.Logging;
using System.Linq;

namespace RelationalGit
{
    public class BlameBasedKnowledgeShareStrategy : BaseKnowledgeShareStrategy
    {
        public BlameBasedKnowledgeShareStrategy(string knowledgeSaveReviewerReplacementType, ILogger logger) 
            : base(knowledgeSaveReviewerReplacementType,  logger)
        {
        }

        protected override DeveloperKnowledge[] SortCandidates(PullRequestContext pullRequestContext, DeveloperKnowledge[] candidates)
        {
            return candidates
            .OrderBy(q => q.NumberOfAuthoredLines)
            .ThenBy(q => q.NumberOfCommits)
            .ThenBy(q => q.NumberOfCommittedFiles).ToArray();
        }
    }
}