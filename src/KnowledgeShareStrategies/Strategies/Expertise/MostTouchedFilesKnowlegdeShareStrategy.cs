using System.Linq;

namespace RelationalGit
{
    public class MostTouchedFilesKnowlegdeShareStrategy : BaseKnowledgeShareStrategy
    {
        public MostTouchedFilesKnowlegdeShareStrategy(string knowledgeSaveReviewerReplacementType)
            : base(knowledgeSaveReviewerReplacementType)
        {
        }

        protected override DeveloperKnowledge[] SortCandidates(PullRequestContext pullRequestContext, DeveloperKnowledge[] candidates)
        {
            return candidates.OrderBy(q => q.NumberOfTouchedFiles).ToArray();
        }
    }
}