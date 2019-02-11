using System.Collections.Generic;
using System.Linq;

namespace RelationalGit
{
    public class FolderLevelSpreadingPlusOneKnowledgeShareStrategy : FolderLevelSpreadingKnowledgeShareStrategy
    {
        public FolderLevelSpreadingPlusOneKnowledgeShareStrategy(string knowledgeSaveReviewerReplacementType)
            : base(knowledgeSaveReviewerReplacementType)
        {
        }

        internal override IEnumerable<(string[] Reviewers, DeveloperKnowledge SelectedCandidateKnowledge)> GetPossibleCandidateSets(PullRequestContext pullRequestContext, DeveloperKnowledge[] availableDevs, PullRequestKnowledgeDistribution prereviewKnowledgeDistribution)
        {
            var reviewers = pullRequestContext.ActualReviewers.Select(q => q.DeveloperName).ToArray();

            for (int i = 0; i < availableDevs.Length; i++)
            {
                yield return (reviewers.Concat( new[] { availableDevs[i].DeveloperName }).ToArray(), availableDevs[i]);
            }
        }
    }
}
