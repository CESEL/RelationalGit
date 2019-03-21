using Microsoft.Extensions.Logging;
using RelationalGit.KnowledgeShareStrategies.Models;
using System.Collections.Generic;
using System.Linq;

namespace RelationalGit
{
    public abstract class SpreadingKnowledgeShareStrategyBase : KnowledgeShareStrategy
    {
        public SpreadingKnowledgeShareStrategyBase(string knowledgeSaveReviewerReplacementType, ILogger logger)
            : base(knowledgeSaveReviewerReplacementType, logger)
        {
        }

        protected override PullRequestRecommendationResult RecommendReviewers(PullRequestContext pullRequestContext)
        {
            var availableDevs = AvailablePRKnowledgeables(pullRequestContext);

            if (availableDevs.Length == 0)
            {
                return new PullRequestRecommendationResult(pullRequestContext.ActualReviewers);
            }

            var simulationResults = new List<PullRequestKnowledgeDistribution>();
            var simulator = new PullRequestReviewSimulator(pullRequestContext, availableDevs,ComputeScore);

            foreach (var candidateSet in GetPossibleCandidateSets(pullRequestContext, availableDevs))
            {
                var simulationResult = simulator.Simulate(candidateSet.Reviewers, candidateSet.SelectedCandidateKnowledge);
                simulationResults.Add(simulationResult);
            }

            if (simulationResults.Count == 0)
            {
                return new PullRequestRecommendationResult(pullRequestContext.ActualReviewers);
            }

            var bestPullRequestKnowledgeDistribution = GetBestDistribution(simulationResults);
            return new PullRequestRecommendationResult(bestPullRequestKnowledgeDistribution.PullRequestKnowledgeDistributionFactors.Reviewers.ToArray(),availableDevs);
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

            return folderLevelKnowlegeables;
        }

        internal abstract IEnumerable<(IEnumerable<DeveloperKnowledge> Reviewers, IEnumerable<DeveloperKnowledge> SelectedCandidateKnowledge)> GetPossibleCandidateSets(PullRequestContext pullRequestContext, DeveloperKnowledge[] availableDevs);

        internal abstract DeveloperKnowledge[] AvailablePRKnowledgeables(PullRequestContext pullRequestContext);

        internal abstract double ComputeScore(PullRequestContext pullRequestContext, PullRequestKnowledgeDistributionFactors pullRequestKnowledgeDistributionFactors);

        internal abstract double ComputeReviewerScore(PullRequestContext pullRequestContext, DeveloperKnowledge reviewer);
    }
}
