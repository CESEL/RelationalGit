using System;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;
using RelationalGit.KnowledgeShareStrategies.Models;

namespace RelationalGit
{


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

        public IEnumerable<DeveloperFileCommitDetail> Details => _map.Values.SelectMany(q => q.Values);
        internal IEnumerable<DeveloperFileCommitDetail> GetCommittersOfPeriod(long periodId)
        {
            return _map.Values.SelectMany(q => q.Values.Where(c => c.Periods.Any(p => p.Id == periodId)));
        }

        public void Add(string filePath,ChangeKind changeKind, Developer developer, Commit commit,Period period)
        {
            var developerName = developer?.NormalizedName;

            if (filePath == null || developerName == null)
            {
                return;
            }

            if (!_map.ContainsKey(filePath))
            {
                _map[filePath] = new Dictionary<string, DeveloperFileCommitDetail>();
            }

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
            {
                _map[filePath][developerName].Periods.Add(period);
            }
        }

        internal bool IsPersonHasCommittedThisFile(string normalizedName, string path)
        {
            var developersFileCommitsDetails = _map.GetValueOrDefault(path);

            if (developersFileCommitsDetails == null)
            {
                return false;
            }

            return developersFileCommitsDetails.Any(q => q.Value.Developer.NormalizedName == normalizedName);
        }

        internal void Remove(string filePath)
        {
            if (_map.ContainsKey(filePath))
            {
                _map.Remove(filePath);
            }
        }
    }
}

