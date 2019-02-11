using System;

namespace RelationalGit
{
    internal class PullRequestKnowledgeDistributionFactors : IComparable<PullRequestKnowledgeDistributionFactors>
    {
        private readonly DeveloperKnowledge[] _availableDevs;

        private DeveloperKnowledge _candidateReviewerKnowledge;

        private double? _score;

        private Func<PullRequestContext, PullRequestKnowledgeDistributionFactors, double> _scoreComputerFunc;

        public PullRequestKnowledgeDistributionFactors(
            string[] reviewers,
            DeveloperKnowledge candidateReviewerKnowledge,
            PullRequestContext pullRequestContext,
            DeveloperKnowledge[] availableDevs,
            Func<PullRequestContext, PullRequestKnowledgeDistributionFactors, double> scoreComputerFunc)
        {
            _availableDevs = availableDevs;
            _candidateReviewerKnowledge = candidateReviewerKnowledge;
            Reviewers = reviewers;
            PullRequestContext = pullRequestContext;
            _scoreComputerFunc = scoreComputerFunc;
        }

        public int FilesAtRisk { get; set; }

        public int AddedKnowledge { get; set; }

        public int TotalKnowledgeables { get; set; }

        public string[] Reviewers { get; }

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
