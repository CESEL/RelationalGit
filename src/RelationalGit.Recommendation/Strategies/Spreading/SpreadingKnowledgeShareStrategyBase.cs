using Microsoft.Extensions.Logging;
using RelationalGit.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RelationalGit.Recommendation
{
    public abstract class SpreadingKnowledgeShareStrategyBase : RecommendationStrategy
    {
        public SpreadingKnowledgeShareStrategyBase(string knowledgeSaveReviewerReplacementType, ILogger logger,bool changePast)
            : base(knowledgeSaveReviewerReplacementType, logger,changePast)
        {
        }

        protected override PullRequestRecommendationResult RecommendReviewers(PullRequestContext pullRequestContext)
        {
            var availableDevs = AvailablePRKnowledgeables(pullRequestContext);

            if (availableDevs.Length == 0)
            {
                return Actual(pullRequestContext);
            }

            var simulationResults = new List<PullRequestKnowledgeDistribution>();
            var simulator = new PullRequestReviewSimulator(pullRequestContext, availableDevs, ComputeScore);

            foreach (var candidateSet in GetPossibleCandidateSets(pullRequestContext, availableDevs))
            {
                var simulationResult = simulator.Simulate(candidateSet.Reviewers, candidateSet.SelectedCandidateKnowledge);
                simulationResults.Add(simulationResult);
            }

            if (simulationResults.Count == 0)
            {
                return Actual(pullRequestContext);
            }

            var bestPullRequestKnowledgeDistribution = GetBestDistribution(simulationResults);

            return Recommendation(pullRequestContext, availableDevs, bestPullRequestKnowledgeDistribution);
        }

        private static PullRequestRecommendationResult Recommendation(PullRequestContext pullRequestContext, DeveloperKnowledge[] availableDevs, PullRequestKnowledgeDistribution bestPullRequestKnowledgeDistribution)
        {
            return new PullRequestRecommendationResult(bestPullRequestKnowledgeDistribution.PullRequestKnowledgeDistributionFactors.Reviewers.ToArray(), availableDevs, pullRequestContext.IsRisky(), pullRequestContext.Features);
        }

        private static PullRequestRecommendationResult Actual(PullRequestContext pullRequestContext)
        {
            return new PullRequestRecommendationResult(pullRequestContext.ActualReviewers, Array.Empty<DeveloperKnowledge>(), pullRequestContext.IsRisky(), pullRequestContext.Features);
        }

        internal PullRequestKnowledgeDistribution GetBestDistribution(List<PullRequestKnowledgeDistribution> simulationResults)
        {
            var maxScore = simulationResults.Max(q => q.PullRequestKnowledgeDistributionFactors.Score);
            return simulationResults.First(q => q.PullRequestKnowledgeDistributionFactors.Score == maxScore);
        }

        protected DeveloperKnowledge[] GetFolderLevelOweners(int depthToScanForReviewers, PullRequestContext pullRequestContext)
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

                var neighbors = blameSnapshot.Trie.GetFileNeighbors(depthToScanForReviewers, actualPath);

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

            foreach (var folderLevelKnowlegeable in folderLevelKnowlegeables)
            {
                folderLevelKnowlegeable.IsFolderLevel = true;
            }

            return folderLevelKnowlegeables;
        }

        internal abstract IEnumerable<(IEnumerable<DeveloperKnowledge> Reviewers, IEnumerable<DeveloperKnowledge> SelectedCandidateKnowledge)> GetPossibleCandidateSets(PullRequestContext pullRequestContext, DeveloperKnowledge[] availableDevs);

        internal abstract DeveloperKnowledge[] AvailablePRKnowledgeables(PullRequestContext pullRequestContext);

        internal abstract double ComputeScore(PullRequestContext pullRequestContext, PullRequestKnowledgeDistributionFactors pullRequestKnowledgeDistributionFactors);

        internal abstract double ComputeReviewerScore(PullRequestContext pullRequestContext, DeveloperKnowledge reviewer);
    }
}
