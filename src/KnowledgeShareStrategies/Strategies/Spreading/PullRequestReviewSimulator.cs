using RelationalGit.KnowledgeShareStrategies.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RelationalGit
{

    internal class PullRequestReviewSimulator
    {
        public PullRequestKnowledgeDistribution PrereviewKnowledgeDistribution = new PullRequestKnowledgeDistribution(null, null, null, null, null);
        private string[] _pullRequestFiles;
        private readonly DeveloperKnowledge[] _availableDevs;
        private readonly PullRequestContext _pullRequestContext;
        private readonly Func<PullRequestContext, PullRequestKnowledgeDistributionFactors, double> _scoreComputerFunc;

        public PullRequestReviewSimulator(
            PullRequestContext pullRequestContext,
            DeveloperKnowledge[] availableDevs,
            Func<PullRequestContext, PullRequestKnowledgeDistributionFactors, double> scoreComputerFunc)
        {
            _availableDevs = availableDevs;
            _pullRequestContext = pullRequestContext;
            _scoreComputerFunc = scoreComputerFunc;

            InitKnowledgeDistribution(pullRequestContext);
        }

        public PullRequestKnowledgeDistribution Simulate(string[] reviewers, DeveloperKnowledge candidateReviewer)
        {
            var simulatedKnowledge = new PullRequestKnowledgeDistribution(reviewers, candidateReviewer, _pullRequestContext, _availableDevs, _scoreComputerFunc);

            foreach (var pullRequestFile in _pullRequestFiles)
            {
                simulatedKnowledge.Add(pullRequestFile, PrereviewKnowledgeDistribution[pullRequestFile], reviewers);
            }

            return simulatedKnowledge;
        }

        private void InitKnowledgeDistribution(PullRequestContext pullRequestContext)
        {
            _pullRequestFiles = pullRequestContext
                .PullRequestFiles
                .Select(q => pullRequestContext.CanononicalPathMapper.GetValueOrDefault(q.FileName)).Where(q => q != null).ToArray();

            foreach (var pullRequestFile in _pullRequestFiles)
            {
                var availableCommitters = pullRequestContext.KnowledgeMap.CommitBasedKnowledgeMap[pullRequestFile]
                    ?.Select(q => q.Key).Where(q => pullRequestContext.AvailableDevelopers.Any(d => d.NormalizedName == q)) ?? Array.Empty<string>();

                var availableReviewers = pullRequestContext.KnowledgeMap.ReviewBasedKnowledgeMap[pullRequestFile]
                     ?.Select(q => q.Key).Where(q => pullRequestContext.AvailableDevelopers.Any(d => d.NormalizedName == q)) ?? Array.Empty<string>();

                PrereviewKnowledgeDistribution.Add(pullRequestFile, availableCommitters.Union(availableReviewers).ToArray());
            }
        }
    }
}
