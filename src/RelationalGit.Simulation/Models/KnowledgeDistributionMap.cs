using System.Collections.Generic;

namespace RelationalGit.Simulation
{
    public class KnowledgeDistributionMap
    {
        public CommitBasedKnowledgeMap CommitBasedKnowledgeMap { get; internal set; }

        public ReviewBasedKnowledgeMap ReviewBasedKnowledgeMap { get; set; }

        public Dictionary<long, PullRequestRecommendationResult> PullRequestSimulatedRecommendationMap { get; internal set; }

        public BlameBasedKnowledgeMap BlameBasedKnowledgeMap { get; internal set; }

        public PullRequestEffortKnowledgeMap PullRequestEffortKnowledgeMap { get; internal set; }
    }
}
