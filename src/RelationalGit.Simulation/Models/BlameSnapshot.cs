using System.Collections.Generic;
using System.Linq;
using RelationalGit.Data;

namespace RelationalGit.Simulation
{
    public class BlameSnapshot
    {
        private readonly Dictionary<string, Dictionary<string, FileBlame>> _map = new Dictionary<string, Dictionary<string, FileBlame>>();
        private readonly Dictionary<string, string> _canonicalToActualPathMapper = new Dictionary<string, string>();

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

        public Dictionary<string, FileBlame> this[string filePath]
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
