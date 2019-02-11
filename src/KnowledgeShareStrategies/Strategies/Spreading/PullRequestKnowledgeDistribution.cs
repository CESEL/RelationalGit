using RelationalGit.KnowledgeShareStrategies.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RelationalGit
{

    internal class PullRequestKnowledgeDistribution : IComparable<PullRequestKnowledgeDistribution>
    {
        private readonly Dictionary<string, string[]> _knowledgeFileDevs = new Dictionary<string, string[]>();
        private readonly Dictionary<string, List<string>> _knowledgeDevFiles = new Dictionary<string, List<string>>();
        private readonly DeveloperKnowledge[] _availableDevs;

        public PullRequestKnowledgeDistributionFactors PullRequestKnowledgeDistributionFactors { get; }

        public DeveloperKnowledge CandidateReviewerKnowledge { get; internal set; }

        public PullRequestKnowledgeDistribution(
            string[] reviewers,
            DeveloperKnowledge candidateReviewerKnowledge,
            PullRequestContext pullRequestContext,
            DeveloperKnowledge[] availableDevs,
            Func<PullRequestContext, PullRequestKnowledgeDistributionFactors, double> scoreComputerFunc)
        {
            _availableDevs = availableDevs;
            PullRequestKnowledgeDistributionFactors = new PullRequestKnowledgeDistributionFactors(reviewers, candidateReviewerKnowledge, pullRequestContext, _availableDevs, scoreComputerFunc);
        }

        public string[] this[string pullRequestFileName] => _knowledgeFileDevs[pullRequestFileName];

        public void Add(string pullRequestFile, string[] knowledgeables)
        {
            _knowledgeFileDevs[pullRequestFile] = knowledgeables;

            UpdateKnowledgeDevFiles(pullRequestFile);

            UpdateDistributionFacors(pullRequestFile, knowledgeables, null);
        }

        private void UpdateKnowledgeDevFiles(string pullRequestFile)
        {
            foreach (var knowledgeable in _knowledgeFileDevs[pullRequestFile])
            {
                if (!_knowledgeDevFiles.ContainsKey(knowledgeable))
                {
                    _knowledgeDevFiles[knowledgeable] = new List<string>(128);
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

            _knowledgeFileDevs[pullRequestFile] = reviewers.Length == 0
                ? existingKnowledgeables
                : existingKnowledgeables.Union(reviewers).ToArray();

            UpdateKnowledgeDevFiles(pullRequestFile);

            UpdateDistributionFacors(pullRequestFile, existingKnowledgeables, reviewers);
        }

        private void UpdateDistributionFacors(string pullRequestFile, string[] existingKnowledgeables, string[] reviewers)
        {
            var knowledgeables = _knowledgeFileDevs[pullRequestFile];

            if (knowledgeables.Length <= 2)
            {
                PullRequestKnowledgeDistributionFactors.FilesAtRisk++;
            }

            PullRequestKnowledgeDistributionFactors.TotalKnowledgeables += knowledgeables.Length;
            PullRequestKnowledgeDistributionFactors.AddedKnowledge += reviewers?.Where(r => existingKnowledgeables.All(e => e != r)).Count() ?? 0;
        }

        public int CompareTo(PullRequestKnowledgeDistribution other)
        {
            return PullRequestKnowledgeDistributionFactors.CompareTo(other.PullRequestKnowledgeDistributionFactors);
        }
    }
}
