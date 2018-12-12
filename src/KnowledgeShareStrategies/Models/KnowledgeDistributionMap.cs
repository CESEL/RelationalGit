using System;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;
using RelationalGit.KnowledgeShareStrategies.Models;

namespace RelationalGit
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

