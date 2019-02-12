using Microsoft.Extensions.Logging;
using System.Linq;

namespace RelationalGit
{
    public class MostTouchedFilesKnowlegdeShareStrategy : BaseKnowledgeShareStrategy
    {
        public MostTouchedFilesKnowlegdeShareStrategy(string knowledgeSaveReviewerReplacementType, ILogger logger)
            : base(knowledgeSaveReviewerReplacementType, logger)
        {
        }

        protected override DeveloperKnowledge[] SortCandidates(PullRequestContext pullRequestContext, DeveloperKnowledge[] candidates)
        {
            return candidates.OrderBy(q => q.NumberOfTouchedFiles).ToArray();
        }
    }
}