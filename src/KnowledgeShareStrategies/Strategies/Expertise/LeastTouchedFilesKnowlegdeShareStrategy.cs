using Microsoft.Extensions.Logging;
using System.Linq;

namespace RelationalGit
{
    public class LeastTouchedFilesKnowlegdeShareStrategy : BaseKnowledgeShareStrategy
    {
        public LeastTouchedFilesKnowlegdeShareStrategy(string knowledgeSaveReviewerReplacementType, ILogger logger)
            : base(knowledgeSaveReviewerReplacementType, logger)
        {
        }

        protected override DeveloperKnowledge[] SortCandidates(PullRequestContext pullRequestContext, DeveloperKnowledge[] candidates)
        {
            return candidates.OrderByDescending(q => q.NumberOfTouchedFiles).ToArray();
        }
    }
}