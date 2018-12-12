using System.Linq;

namespace RelationalGit
{
    public class ReviewBasedKnowledgeShareStrategy : BaseKnowledgeShareStrategy
    {
        public ReviewBasedKnowledgeShareStrategy(string knowledgeSaveReviewerReplacementType)
            : base(knowledgeSaveReviewerReplacementType)
        {
        }

        protected override DeveloperKnowledge[] SortCandidates(PullRequestContext pullRequestContext, DeveloperKnowledge[] candidates)

        {
            return candidates
            .OrderBy(q => q.NumberOfReviews)
            .ThenBy(q => q.NumberOfReviewedFiles)
            .ThenBy(q => q.NumberOfCommits).ToArray();
        }
    }
}