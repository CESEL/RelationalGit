using RelationalGit.KnowledgeShareStrategies.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RelationalGit
{
    public class FolderLevelSpreadingKnowledgeShareStrategy : SpreadingKnowledgeShareStrategyBase
    {
        public FolderLevelSpreadingKnowledgeShareStrategy(string knowledgeSaveReviewerReplacementType) : base(knowledgeSaveReviewerReplacementType)
        { }

        internal override DeveloperKnowledge[] AvailablePRKnowledgeables(PullRequestContext pullRequestContext)
        {
            var folderLevelOwners = GetFolderLevelOweners(pullRequestContext);

            var availableDevs = folderLevelOwners
            .Where(q => q.DeveloperName != pullRequestContext.PRSubmitterNormalizedName &&
            pullRequestContext.AvailableDevelopers.Any(d => d.NormalizedName == q.DeveloperName) &&
            pullRequestContext.ActualReviewers.All(a => a.DeveloperName != q.DeveloperName) &&
            pullRequestContext.Developers[q.DeveloperName].TotalCommits + pullRequestContext.Developers[q.DeveloperName].TotalReviews > 50)
            .ToArray();

            return availableDevs;
        }

        internal override PullRequestKnowledgeDistribution GetBestDistribution(List<RelationalGit.PullRequestKnowledgeDistribution> simulationResults)
        {
            simulationResults.Sort();

            var recommendedSet = simulationResults[simulationResults.Count - 1];

            return recommendedSet;
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

        private DeveloperKnowledge[] GetChangeableReviewers(PullRequestContext pullRequestContext, string[] fixedReviewers)
        {
            return pullRequestContext.ActualReviewers.Where(q => fixedReviewers.Any(f => q.DeveloperName != f)).ToArray();
        }

        private DeveloperKnowledge[] GetFixedReviewers(PullRequestContext pullRequestContext)
        {
            var fixedDevelopers = pullRequestContext.ActualReviewers
                .OrderBy(q => pullRequestContext.Developers[q.DeveloperName].TotalCommits + pullRequestContext.Developers[q.DeveloperName].TotalReviews)
                .TakeLast(1)
                .ToArray();

            return fixedDevelopers;
        }

        internal override bool ShouldRecommend(PullRequestContext pullRequestContext)
        {
            return pullRequestContext.ActualReviewers.Length > 1;      
        }

        private DeveloperKnowledge[] GetFolderLevelOweners(PullRequestContext pullRequestContext)
        {
            var pullRequestFiles = pullRequestContext.PullRequestFiles;
            var blameSnapshot = pullRequestContext.KnowledgeMap.BlameBasedKnowledgeMap.GetSnapshopOfPeriod(pullRequestContext.Period.Id);

            var relatedFiles = new HashSet<string>();

            foreach (var pullRequestFile in pullRequestFiles)
            {
                var canonicalPath = pullRequestContext.CanononicalPathMapper[pullRequestFile.FileName];
                if (canonicalPath == null)
                    continue;

                var actualPath = blameSnapshot.GetActualPath(canonicalPath);

                if (actualPath == null)
                    continue;

                var neighbors = blameSnapshot.Trie.GetFileNeighbors(1, actualPath);

                if (neighbors != null)
                {
                    foreach (var neighbor in neighbors)
                    {
                        relatedFiles.Add(neighbor);
                    }
                }
            }

            var developersKnowledge = new Dictionary<string, DeveloperKnowledge>();

            foreach (var relatedFile in relatedFiles)
            {
                TimeMachine.AddFileOwnership(pullRequestContext.KnowledgeMap, blameSnapshot, developersKnowledge, relatedFile, pullRequestContext.CanononicalPathMapper);
            }

            var folderLevelKnowlegeables = developersKnowledge.Values.Where(q => pullRequestContext.AvailableDevelopers.Any(d => d.NormalizedName == q.DeveloperName)).ToArray();
            // some members of AvailablePRKnowledgeables are not among folderLevelKnowlegeables because folderLevelKnowlegeables contains only the files that we care about their extensions.
            var prKnowledgeables = pullRequestContext.PRKnowledgeables.Where(q => folderLevelKnowlegeables.All(f => f.DeveloperName != q.DeveloperName));

            return folderLevelKnowlegeables.Concat(prKnowledgeables).ToArray();
        }
    }
  
}
