using RelationalGit.KnowledgeShareStrategies.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RelationalGit
{
    public abstract class SpreadingKnowledgeShareStrategyBase : KnowledgeShareStrategy
    {
        public SpreadingKnowledgeShareStrategyBase(string knowledgeSaveReviewerReplacementType) : base(knowledgeSaveReviewerReplacementType)
        {
        }

        protected override PullRequestRecommendationResult RecommendReviewers(PullRequestContext pullRequestContext)
        {
            if (!ShouldRecommend(pullRequestContext))
                return new PullRequestRecommendationResult(pullRequestContext.ActualReviewers);

            var availableDevs = AvailablePRKnowledgeables(pullRequestContext);

            if (availableDevs.Length == 0)
                return new PullRequestRecommendationResult(pullRequestContext.ActualReviewers);

            var simulationResults = new List<PullRequestKnowledgeDistribution>();
            var simulator = new PullRequestReviewSimulator(pullRequestContext, availableDevs);

            foreach (var candidateSet in GetPossibleCandidateSets(pullRequestContext, availableDevs, simulator.PrereviewKnowledgeDistribution))
            {
                var simulationResult = simulator.Simulate(candidateSet.Reviewers, candidateSet.SelectedCandidateKnowledge);
                simulationResults.Add(simulationResult);
            }

            var bestPullRequestKnowledgeDistribution = GetBestDistribution(simulationResults);
            return new PullRequestRecommendationResult(bestPullRequestKnowledgeDistribution.PullRequestKnowledgeDistributionFactors.Reviewers);
        }

        internal abstract bool ShouldRecommend(PullRequestContext pullRequestContext);
        internal abstract PullRequestKnowledgeDistribution GetBestDistribution(List<PullRequestKnowledgeDistribution> simulationResults);
        internal abstract IEnumerable<(string[] Reviewers, DeveloperKnowledge SelectedCandidateKnowledge)> GetPossibleCandidateSets(PullRequestContext pullRequestContext, DeveloperKnowledge[] availableDevs, PullRequestKnowledgeDistribution prereviewKnowledgeDistribution);
        internal abstract DeveloperKnowledge[] AvailablePRKnowledgeables(PullRequestContext pullRequestContext);
    }

    internal class PullRequestReviewSimulator
    {
        public PullRequestKnowledgeDistribution PrereviewKnowledgeDistribution = new PullRequestKnowledgeDistribution(null, null, null,null);
        private string[] _pullRequestFiles;
        private DeveloperKnowledge[] _availableDevs;
        private PullRequestContext _pullRequestContext;

        public PullRequestReviewSimulator(PullRequestContext pullRequestContext, DeveloperKnowledge[] availableDevs)
        {
            _availableDevs = availableDevs;
            _pullRequestContext = pullRequestContext;

            InitKnowledgeDistribution(pullRequestContext);
        }

        public PullRequestKnowledgeDistribution Simulate(string[] reviewers, DeveloperKnowledge candidateReviewer)
        {
            var simulatedKnowledge = new PullRequestKnowledgeDistribution(reviewers, candidateReviewer, _pullRequestContext, _availableDevs);

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
                    ?.Select(q => q.Key).Where(q => pullRequestContext.AvailableDevelopers.Any(d => d.NormalizedName == q));

                var availableReviewers = pullRequestContext.KnowledgeMap.ReviewBasedKnowledgeMap[pullRequestFile]
                     ?.Select(q => q.Key).Where(q => pullRequestContext.AvailableDevelopers.Any(d => d.NormalizedName == q));

                PrereviewKnowledgeDistribution.Add(pullRequestFile, (availableCommitters ?? new string[0]).Union(availableReviewers ?? new string[0]).ToArray());
            }

        }
    }

    internal class PullRequestKnowledgeDistribution : IComparable<PullRequestKnowledgeDistribution>
    {
        private Dictionary<string, string[]> _knowledgeFileDevs = new Dictionary<string, string[]>();
        private Dictionary<string, List<string>> _knowledgeDevFiles = new Dictionary<string, List<string>>();
        private DeveloperKnowledge[] _availableDevs;

        public PullRequestKnowledgeDistributionFactors PullRequestKnowledgeDistributionFactors { get; }
        public DeveloperKnowledge CandidateReviewerKnowledge { get; internal set; }

        public PullRequestKnowledgeDistribution(string[] reviewers, DeveloperKnowledge candidateReviewerKnowledge, PullRequestContext pullRequestContext, DeveloperKnowledge[] availableDevs)
        {
            _availableDevs = availableDevs;
            PullRequestKnowledgeDistributionFactors = new PullRequestKnowledgeDistributionFactors(reviewers, candidateReviewerKnowledge, pullRequestContext, _availableDevs);
        }

        public string[] this[string pullRequestFileName] => _knowledgeFileDevs[pullRequestFileName];

        public void Add(string pullRequestFile, string[] knowledgeables)
        {
            _knowledgeFileDevs[pullRequestFile] = knowledgeables;

            UpdateKnowledgeDevFiles(pullRequestFile, _knowledgeFileDevs[pullRequestFile]);

            UpdateDistributionFacors(pullRequestFile, knowledgeables, null);
        }

        private void UpdateKnowledgeDevFiles(string pullRequestFile, string[] knowledgeables)
        {
            foreach (var knowledgeable in _knowledgeFileDevs[pullRequestFile])
            {
                if (!_knowledgeDevFiles.ContainsKey(knowledgeable))
                {
                    _knowledgeDevFiles[knowledgeable] = new List<string>();
                }

                _knowledgeDevFiles[knowledgeable].Add(pullRequestFile);
            }
        }

        internal void Add(string pullRequestFile, string[] existingKnowledgeables, string[] reviewers)
        {
            if (_knowledgeFileDevs.ContainsKey(pullRequestFile))
            {
                // it happens, we need to find a way to address this.
                //throw new Exception("You are allowed to add a file just once.");
            }

            _knowledgeFileDevs[pullRequestFile] = existingKnowledgeables.Union(reviewers).ToArray();

            UpdateKnowledgeDevFiles(pullRequestFile, _knowledgeFileDevs[pullRequestFile]);

            UpdateDistributionFacors(pullRequestFile, existingKnowledgeables, reviewers);
        }

        private void UpdateDistributionFacors(string pullRequestFile, string[] existingKnowledgeables, string[] reviewers)
        {
            var knowledgeables = _knowledgeFileDevs[pullRequestFile];

            if (knowledgeables.Length <= 3)
            {
                PullRequestKnowledgeDistributionFactors.FilesAtRisk++;
            }

            PullRequestKnowledgeDistributionFactors.TotalKnowledgeables += knowledgeables.Length;
            PullRequestKnowledgeDistributionFactors.AddedKnowledge += reviewers?.Where(r => existingKnowledgeables.All(e => e != r)).Count()??0;
        }

        public int CompareTo(PullRequestKnowledgeDistribution other)
        {
            return PullRequestKnowledgeDistributionFactors.CompareTo(other.PullRequestKnowledgeDistributionFactors);
        }
    }

    internal class PullRequestKnowledgeDistributionFactors : IComparable<PullRequestKnowledgeDistributionFactors>
    {
        private DeveloperKnowledge[] _availableDevs;

        public PullRequestKnowledgeDistributionFactors(string[] reviewers, DeveloperKnowledge candidateReviewerKnowledge, PullRequestContext pullRequestContext, DeveloperKnowledge[] availableDevs)
        {
            _availableDevs = availableDevs;
            CandidateReviewerKnowledge = candidateReviewerKnowledge;
            Reviewers = reviewers;
            PullRequestContext = pullRequestContext;
        }

        public int FilesAtRisk { get; set; }

        public int AddedKnowledge { get; set; }

        public int TotalKnowledgeables { get; set; }

        public DeveloperKnowledge CandidateReviewerKnowledge;

        public string[] Reviewers { get; private set; }

        private PullRequestContext PullRequestContext { get; }

        public int CompareTo(PullRequestKnowledgeDistributionFactors other)
        {
            if (AddedKnowledge > other.AddedKnowledge)
                return 1;
            if (AddedKnowledge < other.AddedKnowledge)
                return -1;

            if (FilesAtRisk > other.FilesAtRisk)
                return 1;
            if (FilesAtRisk < other.FilesAtRisk)
                return -1;

            if (CandidateReviewerKnowledge!= other.CandidateReviewerKnowledge && (CandidateReviewerKnowledge == null || other.CandidateReviewerKnowledge == null))
            {
                var actual = CandidateReviewerKnowledge == null ? this : other;
                var simulated = CandidateReviewerKnowledge == null ? other : this;

                foreach (var actualReviewer in actual.Reviewers)
                {
                    if (!simulated.Reviewers.Any(q => q == actualReviewer))
                    {
                        var developerKnowledge = _availableDevs.SingleOrDefault(q => q.DeveloperName == actualReviewer);

                        if (developerKnowledge != null && developerKnowledge.NumberOfTouchedFiles >= simulated.CandidateReviewerKnowledge.NumberOfTouchedFiles)
                            return (actual == this) ? 1 : -1;
                    }
                }
            }
            else
            {
                if (CandidateReviewerKnowledge?.NumberOfTouchedFiles > other.CandidateReviewerKnowledge?.NumberOfTouchedFiles)
                    return 1;
                if (CandidateReviewerKnowledge?.NumberOfTouchedFiles < other.CandidateReviewerKnowledge?.NumberOfTouchedFiles)
                    return -1;
            }

            if (TotalKnowledgeables > other.TotalKnowledgeables)
                return 1;
            if (TotalKnowledgeables < other.TotalKnowledgeables)
                return -1;

            if (Reviewers.All(q => PullRequestContext.ActualReviewers.Any(q1 => q1.DeveloperName == q)))
                return 1;
            if (other.Reviewers.All(q => PullRequestContext.ActualReviewers.Any(q1 => q1.DeveloperName == q)))
                return -1;

            return 0;
        }
    }

}
