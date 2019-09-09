using RelationalGit.Simulation;
using System;
using System.Collections.Generic;

namespace RelationalGit.Recommendation
{
    internal class PullRequestKnowledgeDistributionFactors : IComparable<PullRequestKnowledgeDistributionFactors>
    {
        private double? _score;

        private Func<PullRequestContext, PullRequestKnowledgeDistributionFactors, double> _scoreComputerFunc;

        public PullRequestKnowledgeDistributionFactors(
            IEnumerable<DeveloperKnowledge> reviewers,
            PullRequestContext pullRequestContext,
            Func<PullRequestContext, PullRequestKnowledgeDistributionFactors, double> scoreComputerFunc)
        {
            Reviewers = reviewers;
            PullRequestContext = pullRequestContext;
            _scoreComputerFunc = scoreComputerFunc;
        }

        public IEnumerable<DeveloperKnowledge> Reviewers { get; }

        private PullRequestContext PullRequestContext { get; }

        public double Score
        {
            get
            {
                if (_score == null)
                {
                    _score = _scoreComputerFunc(PullRequestContext, this);
                }

                return _score.Value;
            }
        }

        public int CompareTo(PullRequestKnowledgeDistributionFactors other)
        {
            return Score.CompareTo(other.Score);
        }
    }
}
