using RelationalGit.Simulation;
using System;
using System.Collections.Generic;

namespace RelationalGit.Recommendation
{

    internal class PullRequestKnowledgeDistribution : IComparable<PullRequestKnowledgeDistribution>
    {
        public PullRequestKnowledgeDistributionFactors PullRequestKnowledgeDistributionFactors { get; }

        public DeveloperKnowledge CandidateReviewerKnowledge { get; internal set; }

        public PullRequestKnowledgeDistribution(
            IEnumerable<DeveloperKnowledge> reviewers,
            PullRequestContext pullRequestContext,
            Func<PullRequestContext, PullRequestKnowledgeDistributionFactors, double> scoreComputerFunc)
        {
            PullRequestKnowledgeDistributionFactors = new PullRequestKnowledgeDistributionFactors(reviewers, pullRequestContext, scoreComputerFunc);
        }

        public int CompareTo(PullRequestKnowledgeDistribution other)
        {
            return PullRequestKnowledgeDistributionFactors.CompareTo(other.PullRequestKnowledgeDistributionFactors);
        }
    }
}
