using Microsoft.Extensions.Logging;
using System.Linq;

namespace RelationalGit
{
    public class ReviewBasedKnowledgeShareStrategy : BaseKnowledgeShareStrategy
    {
        public ReviewBasedKnowledgeShareStrategy(string knowledgeSaveReviewerReplacementType, ILogger logger)
            : base(knowledgeSaveReviewerReplacementType,  logger)
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