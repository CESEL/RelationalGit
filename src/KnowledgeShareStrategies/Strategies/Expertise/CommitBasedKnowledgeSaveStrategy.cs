using System.Linq;

namespace RelationalGit
{
    public class CommitBasedKnowledgeShareStrategy : BaseKnowledgeShareStrategy
    {
        public CommitBasedKnowledgeShareStrategy(string knowledgeSaveReviewerReplacementType)
            : base(knowledgeSaveReviewerReplacementType)
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