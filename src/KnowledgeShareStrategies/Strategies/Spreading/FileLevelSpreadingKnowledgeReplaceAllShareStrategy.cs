using System;
using System.Collections.Generic;
using System.Linq;

namespace RelationalGit.KnowledgeShareStrategies.Strategies.Spreading
{

    public class FileLevelSpreadingKnowledgeReplaceAllShareStrategy : FileLevelSpreadingKnowledgeShareStrategy
    {
        public FileLevelSpreadingKnowledgeReplaceAllShareStrategy(string knowledgeSaveReviewerReplacementType)
            : base(knowledgeSaveReviewerReplacementType)
        {
        }

        internal override IEnumerable<(string[] Reviewers, DeveloperKnowledge SelectedCandidateKnowledge)> GetPossibleCandidateSets(PullRequestContext pullRequestContext, DeveloperKnowledge[] availableDevs, PullRequestKnowledgeDistribution prereviewKnowledgeDistribution)
        {
            var numberOfReviewers = Math.Min(pullRequestContext.ActualReviewers.Length, availableDevs.Length);

            var allPossibleSets = new List<string[]>();
            GetAllPossibleSets(availableDevs, 0, numberOfReviewers, allPossibleSets, new List<string>());

            foreach (var possibleSet in allPossibleSets)
            {
                yield return (possibleSet, null);
            }
        }

        private void GetAllPossibleSets(DeveloperKnowledge[] availableDevs, int currentIndex, int numberOfReviewers, List<string[]> foundSets, List<string> currentSet)
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
