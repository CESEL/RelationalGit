using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using LibGit2Sharp;
using RelationalGit.KnowledgeShareStrategies.Models;

namespace RelationalGit
{
    public class KnowledgeDistributionMap
    {
        public CommitBasedKnowledgeMap CommitBasedKnowledgeMap;
         
        public ReviewBasedKnowledgeMap ReviewBasedKnowledgeMap;
        public Dictionary<long, PullRequestRecommendationResult> PullRequestSimulatedRecommendationMap { get;  set; }
        public BlameBasedKnowledgeMap BlameBasedKnowledgeMap { get; internal set; }
    }

    public class CommitBasedKnowledgeMap
    {
        private Dictionary<string, Dictionary<string, DeveloperFileCommitDetail>> _map = new Dictionary<string, Dictionary<string, DeveloperFileCommitDetail>>();

        public Dictionary<string, DeveloperFileCommitDetail> this[string filePath]
        {
            get
            {
                return _map.GetValueOrDefault(filePath);
            }
        }

        public IEnumerable<DeveloperFileCommitDetail> Details => _map.Values.SelectMany(q=>q.Values);
        internal IEnumerable<DeveloperFileCommitDetail> GetCommittersOfPeriod(long periodId)
        {
            return _map.Values.SelectMany(q => q.Values.Where(c => c.Periods.Any(p => p.Id == periodId)));
        }

        public void Add(string filePath,ChangeKind changeKind, Developer developer, Commit commit,Period period)
        {
            var developerName = developer?.NormalizedName;

            if (filePath == null || developerName == null)
                return;

            if (!_map.ContainsKey(filePath))
                _map[filePath] = new Dictionary<string, DeveloperFileCommitDetail>();

            if (!_map[filePath].ContainsKey(developerName))
            {
                _map[filePath][developerName] = new DeveloperFileCommitDetail()
                {
                    FilePath = filePath,
                    Developer = developer,
                };
            }

            if (!_map[filePath][developerName].Commits.Any(q => q.Sha == commit.Sha))
            {
                _map[filePath][developerName].CommitDetails.Add(new CommitDetail(commit, period, changeKind));
                _map[filePath][developerName].Commits.Add(commit);
            }
            
            if (!_map[filePath][developerName].Periods.Any(q => q.Id == period.Id))
                _map[filePath][developerName].Periods.Add(period);
        }

        internal bool IsPersonHasCommittedThisFile(string normalizedName, string path)
        {
            var developersFileCommitsDetails = _map.GetValueOrDefault(path);

            if (developersFileCommitsDetails == null)
                return false;

            return developersFileCommitsDetails.Any(q => q.Value.Developer.NormalizedName == normalizedName);
        }

        internal void Remove(string filePath)
        {
            if (_map.ContainsKey(filePath))
                _map.Remove(filePath);
        }
    }
    public class ReviewBasedKnowledgeMap
    {
        private Dictionary<string, Dictionary<string, DeveloperFileReveiewDetail>> _map = new Dictionary<string, Dictionary<string, DeveloperFileReveiewDetail>>();

        public Dictionary<string, DeveloperFileReveiewDetail> this[string filePath]
        {
            get
            {
                return _map.GetValueOrDefault(filePath);
            }
        }

        public IEnumerable<DeveloperFileReveiewDetail> Details => _map.Values.SelectMany(q => q.Values);

        public void Add(string filePath, IEnumerable<Developer> reviewersNamesOfPullRequest, PullRequest pullRequest, Period period)
        {
            if (filePath == null)
                return;

            if (!_map.ContainsKey(filePath))
                _map[filePath] = new Dictionary<string, DeveloperFileReveiewDetail>();

            foreach (var reviewer in reviewersNamesOfPullRequest)
            {
                AssignKnowledgeToReviewer(pullRequest, reviewer, period, filePath);
            }
        }

        internal IEnumerable<DeveloperFileReveiewDetail> GetReviewersOfPeriod(long periodId)
        {
            return _map.Values.SelectMany(q => q.Values.Where(c => c.Periods.Any(p => p.Id == periodId)));
        }

        private void AssignKnowledgeToReviewer(PullRequest pullRequest, Developer reviewer, Period period, string filePath)
        {

            var reviewerName = reviewer.NormalizedName;
            
            if (!_map[filePath].ContainsKey(reviewerName))
            {
                _map[filePath][reviewerName] = new DeveloperFileReveiewDetail()
                {
                    FilePath = filePath,
                    Developer = reviewer
                };
            }

            if (!_map[filePath][reviewerName].Periods.Any(q => q.Id == period.Id))
                _map[filePath][reviewerName].Periods.Add(period);

            _map[filePath][reviewerName].PullRequests.Add(pullRequest);
        }

        internal Dictionary<string, DeveloperFileReveiewDetail> GetReviewsOfFile(string filePath)
        {
            return _map.GetValueOrDefault(filePath);
        }
    }
    public class BlameBasedKnowledgeMap
    {
        private Dictionary<long, BlameSnapshot> map = new Dictionary<long, BlameSnapshot>();
        public void Add(Period period,string filePath, string developerName, CommitBlobBlame commitBlobBlame)
        {
            var periodId = period.Id;

            if (!map.ContainsKey(periodId))
            {
                map[periodId] = new BlameSnapshot();
            }

            map[periodId].Add(period, filePath, developerName, commitBlobBlame);
        }

        public void ComputeOwnershipPercentage()
        {
            foreach (var periodId in map.Keys)
            {
                map[periodId].ComputeOwnershipPercentage();
            }
        }

        internal BlameSnapshot GetSnapshopOfPeriod(long periodId)
        {
            return map.GetValueOrDefault(periodId);
        }
    }

    public class BlameSnapshot
    {
        private Dictionary<string, Dictionary<string, FileBlame>> _map = new Dictionary<string, Dictionary<string, FileBlame>>();
        private Dictionary<string, string> _canonicalToActualPathMapper = new Dictionary<string, string>();

        public BlameSnapshot()
        {
            Trie = new DirectoryTrie();
        }

        public void Add(Period period, string filePath, string developerName, CommitBlobBlame commitBlobBlame)
        {

            if (!_map.ContainsKey(filePath))
            {
                _map[filePath] = new Dictionary<string, FileBlame>();
                Trie.Add(commitBlobBlame.Path); // we build the Trie using the actual file paths. (filePath variable contains canonical path here.)
                _canonicalToActualPathMapper[commitBlobBlame.CanonicalPath] = commitBlobBlame.Path;
            }

            if (!_map[filePath].ContainsKey(developerName))
            {
                _map[filePath][developerName] = new FileBlame()
                {
                    FileName = filePath,
                    Period = period,
                    NormalizedDeveloperName = developerName
                };
            }

            _map[filePath][developerName].TotalAuditedLines += commitBlobBlame.AuditedLines;
        }

        public void ComputeOwnershipPercentage()
        {
            foreach (var filePath in _map.Keys)
            {
                var totalLines = (double)_map[filePath].Values.Sum(q => q.TotalAuditedLines);

                foreach (var developer in _map[filePath].Keys)
                {
                    _map[filePath][developer].OwnedPercentage = _map[filePath][developer].TotalAuditedLines / totalLines;
                }
            }
        }

        public IEnumerable<string> FilePaths => _map.Keys;

        public Dictionary<string, FileBlame>  this[string filePath]
        {
            get
            {
                return _map.GetValueOrDefault(filePath);
            }
        }
        
        public string GetActualPath(string canonicalPath)
        {
            return _canonicalToActualPathMapper.GetValueOrDefault(canonicalPath);
        }

        public DirectoryTrie Trie { get; }
    }
}

