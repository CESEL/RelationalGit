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
            return pullRequestContext.ActualReviewers.Where(q => fixedReviewers.Any(f => q.DeveloperName != f)).ToArray();
        }

        private DeveloperKnowledge[] GetFixedReviewers(PullRequestContext pullRequestContext)
        {
            return pullRequestContext.ActualReviewers
                .OrderBy(q => pullRequestContext.Developers[q.DeveloperName].TotalCommits + pullRequestContext.Developers[q.DeveloperName].TotalReviews)
                .TakeLast(1)
                .ToArray();
        }

        internal override bool ShouldRecommend(PullRequestContext pullRequestContext)
        {
            return pullRequestContext.ActualReviewers.Length > 1;
        }

        internal override PullRequestKnowledgeDistribution GetBestDistribution(List<PullRequestKnowledgeDistribution> simulationResults)
        {
            simulationResults.Sort();

            return simulationResults[simulationResults.Count - 1];
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
            return pullRequestContext.PRKnowledgeables.Where(q => q.DeveloperName != pullRequestContext.PRSubmitterNormalizedName &&
              IsDeveloperAvailable(pullRequestContext, q.DeveloperName) &&
              !IsDevelperAmongActualReviewers(pullRequestContext, q.DeveloperName) &&
              IsCoreDeveloper(pullRequestContext, q.DeveloperName)).ToArray();
        }
    }

    public class FileLevelSpreadingKnowledgeReplaceAllShareStrategy : FileLevelSpreadingKnowledgeShareStrategy
    {
        public FileLevelSpreadingKnowledgeReplaceAllShareStrategy(string knowledgeSaveReviewerReplacementType)
            : base(knowledgeSaveReviewerReplacementType)
        {
        }

        internal override bool ShouldRecommend(PullRequestContext pullRequestContext)
        {
            return pullRequestContext.ActualReviewers.Length > 0;
        }

        internal override IEnumerable<(string[] Reviewers, DeveloperKnowledge SelectedCandidateKnowledge)> GetPossibleCandidateSets(PullRequestContext pullRequestContext, DeveloperKnowledge[] availableDevs, PullRequestKnowledgeDistribution prereviewKnowledgeDistribution)
        {
            var numberOfReviewers = Math.Min(pullRequestContext.ActualReviewers.Length,availableDevs.Length);

            var allPossibleSets = new List<string[]>();
            GetAllPossibleSets(availableDevs,0,numberOfReviewers, allPossibleSets, new List<string>());

            foreach (var possibleSet in allPossibleSets)
            {
                yield return (possibleSet, null);
            }
        }

        private void GetAllPossibleSets(DeveloperKnowledge[] availableDevs,int currentIndex, int numberOfReviewers, List<string[]> foundSets, List<string> currentSet)
        {
            if (numberOfReviewers == 0)
            {
                return;
            }

            for (int i = currentIndex; i < availableDevs.Length - numberOfReviewers + 1; i++)
            {
                currentSet.Add(availableDevs[i].DeveloperName);

                if (numberOfReviewers == 1)
                {
                    foundSets.Add(currentSet.ToArray());
                }
                else
                {
                    GetAllPossibleSets(availableDevs, i + 1, numberOfReviewers - 1, foundSets, currentSet);
                }

                currentSet.RemoveAt(currentSet.Count - 1);
            }
        }
    }
}
