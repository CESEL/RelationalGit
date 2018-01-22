
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using AutoMapper;

namespace RelationalGit
{
    public class GitRepository
    {
        #region Fields

        private Repository _gitRepo;
        private string _localClonePath;
        private Dictionary<string, string> _fileOidHolder = new Dictionary<string, string>();
        private HashSet<string> _trackedChanges = new HashSet<string>();

        #endregion

        public GitRepository(string localRepositoryPath)
        {
            _gitRepo = new Repository(localRepositoryPath);
            _localClonePath = localRepositoryPath;
        }

        #region Public Interface

        public Commit[] ExtractCommitsFromBranch(string branchName = "master")
        {
            Ensure.ArgumentNotNullOrEmptyString(branchName,nameof(branchName));

            var branch = _gitRepo.Branches[branchName];

            var filter = new CommitFilter
            {
                SortBy = CommitSortStrategies.Topological
                | CommitSortStrategies.Time
                | CommitSortStrategies.Reverse,
                IncludeReachableFrom = branch
            };

            var commits = _gitRepo
                .Commits
                .QueryBy(filter)
                .ToArray();

            var extractedCommits = Mapper.Map<Commit[]>(commits);

            return extractedCommits;
        }

        public void LoadChangesOfCommits(Commit[] orderedCommits)
        {
            _trackedChanges.Clear();
            _fileOidHolder.Clear();

            for (int i = 0; i < orderedCommits.Length; i++)
            {
                LoadChangesOfCommit(orderedCommits[i]);
            }
        }

        public void LoadBlobsAndTheirBlamesOfCommit(Commit commit, string[] validExtensions, Dictionary<string, string> canonicalPathDic, string branchName = "master")
        {
            var blobsPath = GetBlobsPathFromCommitTree(commit.GitCommit.Tree, validExtensions);            
            var committedBlobs = new ConcurrentBag<CommittedBlob>();

            Parallel.ForEach(
                blobsPath,
                new ParallelOptions() {MaxDegreeOfParallelism=4},
                () =>
                {
                    return  GetPowerShellInstance();
                },
                (blobPath, i, powerShellInstance) =>
                {
                    if (canonicalPathDic.ContainsKey(blobPath))
                    {
                        var blobBlames = GetBlobBlamesOfCommit(powerShellInstance, commit.Sha, blobPath, canonicalPathDic);

                        committedBlobs.Add(new CommittedBlob()
                        {
                            NumberOfLines = blobBlames.Sum(m => m.AuditedLines),
                            Path = blobPath,
                            CommitSha = commit.Sha,
                            CanonicalPath = canonicalPathDic[blobPath],
                            CommitBlobBlames = blobBlames
                        });
                    }

                    return powerShellInstance;
                },
                (powerShellInstance) =>
                {
                    powerShellInstance.Dispose();
                });

            commit.Blobs = committedBlobs.ToList();
        }


        #endregion

        private ICollection<CommitBlobBlame> GetBlobBlamesOfCommit(PowerShell powerShellInstance,string commitSha, string blobPath, Dictionary<string, string> canonicalPathDic)
        {
            var rawBlameLines = GetBlameFromPowerShell(powerShellInstance, commitSha, blobPath);

            var blobBlames = ParseRawBlameLines(rawBlameLines);

            foreach (var blobBlame in blobBlames)
            {
                blobBlame.CommitSha = commitSha;
                blobBlame.CanonicalPath = canonicalPathDic[blobPath];
            }

            return blobBlames;
        }

        private void LoadChangesOfCommit(Commit commit)
        {
            var committedChanges = new List<CommittedChange>();

            var gitCommit = commit.GitCommit;

            var compareOptions = new CompareOptions
            {
                Algorithm = DiffAlgorithm.Minimal,
                Similarity = new SimilarityOptions
                {
                    RenameDetectionMode = RenameDetectionMode.Exact,
                    RenameLimit = 9999
                }
            };

            if (gitCommit.Parents.Count() <= 1)
                committedChanges = GetDiffOfTrees(_gitRepo, gitCommit.Parents.SingleOrDefault()?.Tree, gitCommit.Tree, compareOptions);
            else
            {
                foreach (var parent in gitCommit.Parents)
                    committedChanges.AddRange(GetDiffOfTrees(_gitRepo, parent.Tree, gitCommit.Tree, compareOptions));

                var items = committedChanges
                    .Where(m => m.Status == (short)ChangeKind.Deleted)
                    .GroupBy(c => c.Path)
                    .Where(grp => grp.Count() == 1)
                    .Select(m => m.Max(c => c)).ToArray();

                foreach (var item in items)
                {
                    committedChanges.Remove(item);
                }

                committedChanges = committedChanges.GroupBy(i => i.Path)
                    .Select(g => g.First()).ToList();
            }

            foreach (var committedFile in committedChanges)
                committedFile.CommitSha = commit.Sha;

            commit.CommittedChanges = committedChanges;
        }

        private ICollection<CommitBlobBlame> ParseRawBlameLines(string[] blameLines)
        {
            var blobBlames = new List<CommitBlobBlame>();

            var developerLineOwnershipDictionary = new Dictionary<string, int>();

            foreach (var blameLine in blameLines)
            {
                var blameRegex = new Regex(@"(?<sha>\w{40})\s+\(<(?<email>[\w\.\-]+@[\w\-]+\.\w{2,3})>\s+(?<datetime>\d\d\d\d-\d\d-\d\d\s\d\d\:\d\d:\d\d\s(-|\+)\d\d\d\d)\s+(?<lineNumber>\d+)\)\.*");
                var mc = blameRegex.Matches(blameLine);

                if (mc.Count == 0)
                    continue;

                if (!developerLineOwnershipDictionary.ContainsKey(mc[0].Groups["email"].Value))
                    developerLineOwnershipDictionary[mc[0].Groups["email"].Value] = 0;

                developerLineOwnershipDictionary[mc[0].Groups["email"].Value]++;
            }

            foreach (var developerLineOwnership in developerLineOwnershipDictionary)
            {
                blobBlames.Add(new CommitBlobBlame()
                {
                    AuditedLines = developerLineOwnership.Value,
                    DeveloperIdentity = developerLineOwnership.Key,
                    AuditedPercentage = developerLineOwnership.Value / (double)blameLines.Length
                });
            }

            return blobBlames;
        }
        private string[] GetBlameFromPowerShell(PowerShell powerShellInstance, string commitSha, string blobPath)
        {
            powerShellInstance.Commands.Clear();
            powerShellInstance.AddScript($@"git blame -l -w -e -c {commitSha} -- '{blobPath}'");
            var blameLines = powerShellInstance.Invoke().Select(m => m.ToString()).ToArray();
            return blameLines;
        }

        private PowerShell GetPowerShellInstance()
        {
            var powerShellInstance = PowerShell.Create();
            powerShellInstance.AddScript($@"set-location '{_localClonePath}'");
            powerShellInstance.Invoke();
            
            return powerShellInstance;
        }

        private List<string> GetBlobsPathFromCommitTree(Tree tree,string[] validExtensions)
        {
            var result = new List<string>();

            var blobs = tree.Where(m => m.TargetType == TreeEntryTargetType.Blob
            && m.Mode == Mode.NonExecutableFile
            && validExtensions.Any(f=> m.Name.EndsWith(f)));

            result.AddRange(blobs.Select(m => m.Path).ToArray());

            foreach (var treeEntry in tree.Where(m => m.TargetType == TreeEntryTargetType.Tree))
            {
                result.AddRange(GetBlobsPathFromCommitTree((Tree)treeEntry.Target,validExtensions));
            }

            return result;
        }

        private List<CommittedChange> GetDiffOfTrees(LibGit2Sharp.Repository repo, Tree oldTree
            , Tree newTree, CompareOptions compareOptions)
        {
            var result = new List<CommittedChange>();

            foreach (TreeEntryChanges change in repo.Diff.Compare<TreeChanges>(oldTree, newTree, compareOptions))
            {
                var changeStatus = change.Status;
                if (changeStatus == ChangeKind.Copied)
                    changeStatus = ChangeKind.Added;

                if (changeStatus == ChangeKind.Unmodified)
                    continue;

                var key = "";

                if (changeStatus == ChangeKind.Deleted)
                    key = change.OldOid.Sha + change.Path + changeStatus;
                else
                    key = change.Oid.Sha + change.Path + changeStatus;

                if (_trackedChanges.Contains(key))
                    continue;

                if (changeStatus == ChangeKind.Added)
                {
                    if (_trackedChanges.Contains(change.Oid.Sha + change.Path + ChangeKind.Modified))
                        continue;
                    if (_trackedChanges.Contains(change.Oid.Sha + change.Path + ChangeKind.Renamed))
                        continue;
                }

                _trackedChanges.Add(key);

                string canonicalPath;

                if (change.OldExists && changeStatus!=ChangeKind.Added)
                {
                    canonicalPath = _fileOidHolder[change.OldPath];
                    _fileOidHolder[change.Path] = canonicalPath;
                }
                else
                    canonicalPath = _fileOidHolder[change.Path] = change.Path;

                var committedFile = new CommittedChange
                {
                    CanonicalPath = canonicalPath,
                    Status = (short)changeStatus,
                    Oid = change.Oid.Sha,
                    OldPath = change.OldPath,
                    Path = change.Path,
                    OldOid = change.OldOid.Sha
                };

                result.Add(committedFile);
            }

            return result;
        }
    }
}
