using RelationalGit.Simulation;
using System;
using System.Collections.Generic;

namespace RelationalGit.Recommendation
{
    internal class PullRequestReviewSimulator
    {
        private readonly PullRequestContext _pullRequestContext;
        private readonly Func<PullRequestContext, PullRequestKnowledgeDistributionFactors, double> _scoreComputerFunc;

        public PullRequestReviewSimulator(
            PullRequestContext pullRequestContext,
            DeveloperKnowledge[] availableDevs,
            Func<PullRequestContext, PullRequestKnowledgeDistributionFactors, double> scoreComputerFunc)
        {
            _pullRequestContext = pullRequestContext;
            _scoreComputerFunc = scoreComputerFunc;
        }

        public PullRequestKnowledgeDistribution Simulate(IEnumerable<DeveloperKnowledge> reviewers, IEnumerable<DeveloperKnowledge> selectedReviewers)
        {
            var simulatedKnowledge = new PullRequestKnowledgeDistribution(reviewers,  _pullRequestContext, _scoreComputerFunc);
            return simulatedKnowledge;
        }
    }
}
