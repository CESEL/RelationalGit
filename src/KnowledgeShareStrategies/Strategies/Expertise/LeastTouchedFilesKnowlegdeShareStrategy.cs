using System.Linq;

namespace RelationalGit
{
    public class LeastTouchedFilesKnowlegdeShareStrategy : BaseKnowledgeShareStrategy
    {
        public LeastTouchedFilesKnowlegdeShareStrategy(string knowledgeSaveReviewerReplacementType)
            : base(knowledgeSaveReviewerReplacementType)
        {
        }

        protected override DeveloperKnowledge[] SortCandidates(PullRequestContext pullRequestContext, DeveloperKnowledge[] candidates)
        {
            return candidates.OrderByDescending(q => q.NumberOfTouchedFiles).ToArray();
        }
    }
}