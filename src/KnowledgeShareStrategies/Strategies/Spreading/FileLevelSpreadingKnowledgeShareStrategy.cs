using System;
using System.Collections.Generic;
using System.Linq;

namespace RelationalGit.KnowledgeShareStrategies.Strategies.Spreading
{
    public class FileLevelSpreadingKnowledgeShareStrategy : SpreadingKnowledgeShareStrategyBase
    {
        public FileLevelSpreadingKnowledgeShareStrategy(string knowledgeSaveReviewerReplacementType)
            : base(knowledgeSaveReviewerReplacementType)
        {
        }

        private DeveloperKnowledge[] GetChangeableReviewers(PullRequestContext pullRequestContext, string[] fixedReviewers)
        {
            return pullRequestContext.ActualReviewers.Where(q => fixedReviewers.All(f => q.DeveloperName != f)).ToArray();
        }

        private DeveloperKnowledge[] GetFixedReviewers(PullRequestContext pullRequestContext)
        {
            return pullRequestContext.ActualReviewers
                .OrderByDescending(q => pullRequestContext.Developers[q.DeveloperName].TotalCommits + pullRequestContext.Developers[q.DeveloperName].TotalReviews)
                .Take(1).ToArray();
        }

        internal override IEnumerable<(string[] Reviewers, DeveloperKnowledge SelectedCandidateKnowledge)> GetPossibleCandidateSets(PullRequestContext pullRequestContext, DeveloperKnowledge[] availableDevs, PullRequestKnowledgeDistribution prereviewKnowledgeDistribution)
        {
            var fixedReviewers = GetFixedReviewers(pullRequestContext).Select(q => q.DeveloperName).ToArray();
            var changeableReviewers = GetChangeableReviewers(pullRequestContext, fixedReviewers).Select(q => q.DeveloperName).ToArray();
            var changeableReviewersBase = (string[])changeableReviewers.Select(q => q).ToArray().Clone();

            yield return ((string[])fixedReviewers.Concat(changeableReviewersBase).ToArray().Clone(), null);

            for (int i = 0; i < changeableReviewersBase.Length; i++)
            {
                var actualReviewer = changeableReviewersBase[i];

                foreach (var candidateReviewer in availableDevs)
                {
                    changeableReviewersBase[i] = candidateReviewer.DeveloperName;
                    yield return ((string[])fixedReviewers.Concat(changeableReviewersBase).ToArray().Clone(), candidateReviewer);
                }

                changeableReviewersBase[i] = actualReviewer;
            }
        }

        internal override DeveloperKnowledge[] AvailablePRKnowledgeables(PullRequestContext pullRequestContext)
        {
            return pullRequestContext.PullRequestKnowledgeables.Where(q => q.DeveloperName != pullRequestContext.PRSubmitterNormalizedName &&
              IsDeveloperAvailable(pullRequestContext, q.DeveloperName) &&
              !IsDevelperAmongActualReviewers(pullRequestContext, q.DeveloperName) &&
              IsCoreDeveloper(pullRequestContext, q.DeveloperName)).ToArray();
        }

        internal override double ComputeScore(PullRequestContext pullRequestContext, PullRequestKnowledgeDistributionFactors pullRequestKnowledgeDistributionFactors)
        {
            return (double)pullRequestKnowledgeDistributionFactors.AddedKnowledge / (pullRequestKnowledgeDistributionFactors.FilesAtRisk + 1.0);
        }
    }
}
