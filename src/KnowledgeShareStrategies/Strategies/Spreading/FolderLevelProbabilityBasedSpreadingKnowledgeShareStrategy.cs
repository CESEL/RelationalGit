using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RelationalGit.KnowledgeShareStrategies.Strategies.Spreading
{
    public class FolderLevelProbabilityBasedSpreadingKnowledgeShareStrategy : FileLevelProbabilityBasedSpreadingKnowledgeShareStrategy
    {
        private int? _numberOfPeriodsForCalculatingProbabilityOfStay;
        private bool? _addOnlyToUnsafePullrequests;
        private readonly PullRequestReviewerSelectionStrategy[] _pullRequestReviewerSelectionStrategies;
        private readonly PullRequestReviewerSelectionStrategy _pullRequestReviewerSelectionDefaultStrategy;
        private static Dictionary<string, int[][]> _combinationDic = new Dictionary<string, int[][]>();

        public FolderLevelProbabilityBasedSpreadingKnowledgeShareStrategy(string knowledgeSaveReviewerReplacementType, ILogger logger, int? numberOfPeriodsForCalculatingProbabilityOfStay, string pullRequestReviewerSelectionStrategy, bool? addOnlyToUnsafePullrequests)
            : base(knowledgeSaveReviewerReplacementType, logger, numberOfPeriodsForCalculatingProbabilityOfStay, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests)
        {
        }

        internal override DeveloperKnowledge[] AvailablePRKnowledgeables(PullRequestContext pullRequestContext)
        {
            var folderLevelOwners = GetFolderLevelOweners(pullRequestContext);

            return folderLevelOwners.Where(q => q.DeveloperName != pullRequestContext.PRSubmitterNormalizedName &&
              IsDeveloperAvailable(pullRequestContext, q.DeveloperName) &&
             // !IsDevelperAmongActualReviewers(pullRequestContext, q.DeveloperName) &&
              IsCoreDeveloper(pullRequestContext, q.DeveloperName)).ToArray();
        }

        private DeveloperKnowledge[] GetFolderLevelOweners(PullRequestContext pullRequestContext)
        {
            var pullRequestFiles = pullRequestContext.PullRequestFiles;
            var blameSnapshot = pullRequestContext.KnowledgeMap.BlameBasedKnowledgeMap.GetSnapshopOfPeriod(pullRequestContext.PullRequestPeriod.Id);

            var relatedFiles = new HashSet<string>();

            foreach (var pullRequestFile in pullRequestFiles)
            {
                var canonicalPath = pullRequestContext.CanononicalPathMapper[pullRequestFile.FileName];
                if (canonicalPath == null)
                {
                    continue;
                }

                var actualPath = blameSnapshot.GetActualPath(canonicalPath);

                if (actualPath == null)
                {
                    continue;
                }

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
            var prKnowledgeables = pullRequestContext.PullRequestKnowledgeables.Where(q => folderLevelKnowlegeables.All(f => f.DeveloperName != q.DeveloperName));

            return folderLevelKnowlegeables.Concat(prKnowledgeables).ToArray();
        }
    }
}
