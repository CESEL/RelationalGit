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
            return new PullRequestRecommendationResult(bestPullRequestKnowledgeDistribution.PullRequestKnowledgeDistributionFactors.Reviewers.ToArray());
        }

        internal PullRequestKnowledgeDistribution GetBestDistribution(List<PullRequestKnowledgeDistribution> simulationResults)
        {
            var maxScore = simulationResults.Max(q => q.PullRequestKnowledgeDistributionFactors.Score);
            return simulationResults.First(q => q.PullRequestKnowledgeDistributionFactors.Score == maxScore);
        }

        internal abstract IEnumerable<(IEnumerable<DeveloperKnowledge> Reviewers, IEnumerable<DeveloperKnowledge> SelectedCandidateKnowledge)> GetPossibleCandidateSets(PullRequestContext pullRequestContext, DeveloperKnowledge[] availableDevs);

        internal abstract DeveloperKnowledge[] AvailablePRKnowledgeables(PullRequestContext pullRequestContext);

        internal abstract double ComputeScore(PullRequestContext pullRequestContext, PullRequestKnowledgeDistributionFactors pullRequestKnowledgeDistributionFactors);
    }
}
