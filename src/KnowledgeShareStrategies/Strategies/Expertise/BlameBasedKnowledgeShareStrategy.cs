using System.Linq;

namespace RelationalGit
{
    public class BlameBasedKnowledgeShareStrategy : BaseKnowledgeShareStrategy
    {
        public BlameBasedKnowledgeShareStrategy(string knowledgeSaveReviewerReplacementType) 
            : base(knowledgeSaveReviewerReplacementType)
        {
        }

        protected override DeveloperKnowledge[] SortCandidates(PullRequestContext pullRequestContext,DeveloperKnowledge[] candidates)
        {
            return candidates
            .OrderBy(q => q.NumberOfAuthoredLines)
            .ThenBy(q => q.NumberOfCommits)
            .ThenBy(q => q.NumberOfCommittedFiles).ToArray();
        }
    }
}