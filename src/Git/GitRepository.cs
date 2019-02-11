
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace RelationalGit
{
    public class GitRepository : IDisposable
    {
        #region Fields

        private readonly Repository _gitRepo;
        private readonly string _localClonePath;
        private readonly ILogger _logger;

        #endregion

        public GitRepository(string localRepositoryPath, ILogger logger)
        {
            _gitRepo = new Repository(localRepositoryPath);
            _localClonePath = localRepositoryPath;
            _logger = logger;
        }

        #region Public Interface

        public Commit[] ExtractCommitsFromBranch(string branchName = "master")
        {
            Ensure.ArgumentNotNullOrEmptyString(branchName, nameof(branchName));

            var branch = _gitRepo.Branches[branchName];

            if (branch == null)
            {
                throw new Exception($"No branch with name {branchName}");
            }

            var filter = new CommitFilter
            {
                SortBy = CommitSortStrategies.Topological
                | CommitSortStrategies.Time
                | CommitSortStrategies.Reverse,
                IncludeReachableFrom = branch
            };

            _logger.LogInformation("{datetime}: getting commits from git.", DateTime.Now);

            var commits = _gitRepo.Commits.QueryBy(filter).ToArray();

            return Mapper.Map<Commit[]>(commits);
        }

        public void LoadChangesOfCommits(Commit[] orderedCommits)
        {
            var fileOidHolder = new Dictionary<string, string>();
            var pathFilesDic = new Dictionary<string, List<CommittedChange>>();

            _logger.LogInformation("{datetime}: trying to get committed changes from all the {count} commits.", DateTime.Now, orderedCommits.Length);

            for (int i = 0; i < orderedCommits.Length; i++)
            {
                if (i % 500 == 0)
                {
                    _logger.LogInformation("{dateTime}: more than {count} commits have been processed", DateTime.Now, i);
                }

                LoadChangesOfCommit(orderedCommits[i], fileOidHolder, pathFilesDic);
            }

            // assigning canonical paths to changes all at once at the end of the operation.
            foreach (var kv in pathFilesDic)
            {
                foreach (var change in kv.Value)
                {
                    change.CanonicalPath = kv.Key;
                }
            }
        }

        public void LoadBlobsAndTheirBlamesOfCommit(
            Commit commit,
            string[] validExtensions,
            string[] excludedPaths,
            Dictionary<string, string> canonicalPathDic,
            bool extractBlames,
            string branchName = "master")
        {
            var commitsDic = ExtractCommitsFromBranch(branchName).ToDictionary(q => q.Sha);

            var blobsPath = GetBlobsPathFromCommitTree(commit.GitCommit.Tree, validExtensions, excludedPaths);

            var committedBlobs = new ConcurrentBag<CommittedBlob>();

            Parallel.ForEach(
                blobsPath,
                new ParallelOptions() { MaxDegreeOfParallelism = 5 },
                GetPowerShellInstance,
                (blobPath, _, powerShellInstance) =>
                {
                    var blobBlames = GetBlobBlamesOfCommit(powerShellInstance, commit.Sha, blobPath, canonicalPathDic, commitsDic, extractBlames);

                    committedBlobs.Add(new CommittedBlob()
                    {
                        NumberOfLines = blobBlames.Sum(m => m.AuditedLines),
                        Path = blobPath,
                        CommitSha = commit.Sha,
                        CanonicalPath = canonicalPathDic.GetValueOrDefault(blobPath),
                        CommitBlobBlames = blobBlames
                    });

                    if (committedBlobs.Count() % 500 == 0)
                    {
                        _logger.LogInformation("{datetime} : Blames extraction from {currentCount} files out of {total} completed.", DateTime.Now, committedBlobs.Count, blobsPath.Count);
                    }

                    return powerShellInstance;
                },
                (powerShellInstance) => powerShellInstance?.Dispose());

            commit.Blobs = committedBlobs.ToList();
        }

        #endregion

        private ICollection<CommitBlobBlame> GetBlobBlamesOfCommit(
            PowerShell powerShellInstance,
            string commitSha,
            string blobPath,
            Dictionary<string, string> canonicalPathDic,
            Dictionary<string, Commit> commitsDic,
            bool extractBlames)
        {
            if (!extractBlames)
            {
                return Array.Empty<CommitBlobBlame>();
            }

            var rawBlameLines = GetBlameFromPowerShell(powerShellInstance, commitSha, blobPath);

            return ExtraxtBlames(rawBlameLines, blobPath, commitSha, canonicalPathDic, commitsDic);
        }

        private void LoadChangesOfCommit(Commit commit, Dictionary<string, string> fileOidHolder, Dictionary<string, List<CommittedChange>> pathFilesDic)
        {
            var committedChanges = new List<CommittedChange>();

            var gitCommit = commit.GitCommit;

            var compareOptions = new CompareOptions
            {
                Algorithm = DiffAlgorithm.Minimal,
                Similarity = new SimilarityOptions
                {
                    RenameDetectionMode = RenameDetectionMode.Renames,
                }
            };

            if (gitCommit.Parents.Count() <= 1)
            {
                committedChanges.AddRange(GetDiffOfTrees(_gitRepo, gitCommit.Parents.SingleOrDefault()?.Tree, gitCommit.Tree, compareOptions, fileOidHolder, pathFilesDic));
            }
            else
            {
                committedChanges.AddRange(GetDiffOfMergedTrees(_gitRepo, gitCommit.Parents, gitCommit.Tree, compareOptions, fileOidHolder, pathFilesDic));
            }

            ReorderChanges(commit, fileOidHolder, pathFilesDic, committedChanges);

            commit.CommittedChanges = committedChanges;
        }

        private static void ReorderChanges(Commit commit, Dictionary<string, string> fileOidHolder, Dictionary<string, List<CommittedChange>> pathFilesDic, List<CommittedChange> committedChanges)
        {
            foreach (var committedFile in committedChanges)
            {
                committedFile.CommitSha = commit.Sha;
                var canonicalPath = committedFile.CanonicalPath;

                // adding new changes to the dictionary
                if (!pathFilesDic.ContainsKey(canonicalPath))
                {
                    pathFilesDic[canonicalPath] = new List<CommittedChange>();
                }

                pathFilesDic[canonicalPath].Add(committedFile);
            }

            // reordering changes to their new canonical address. 
            // so, files with same path/oldpath will have a same canonical address
            foreach (var committedChange in committedChanges)
            {
                if (committedChange.Path == committedChange.OldPath)
                {
                    continue;
                }

                var pathCanonical = fileOidHolder[committedChange.Path];
                var oldPathCanonical = fileOidHolder[committedChange.OldPath];

                if (pathCanonical != oldPathCanonical)
                {
                    pathFilesDic[pathCanonical] = pathFilesDic[pathCanonical].Concat(pathFilesDic[oldPathCanonical]).ToList();
                    pathFilesDic.Remove(oldPathCanonical);

                    fileOidHolder[oldPathCanonical] = pathCanonical;

                    foreach (var item in pathFilesDic[pathCanonical])
                    {
                        fileOidHolder[item.Path] = pathCanonical;
                        fileOidHolder[item.OldPath] = pathCanonical;
                    }
                }
            }
        }

        private IEnumerable<CommittedChange> GetDiffOfMergedTrees(Repository gitRepo, IEnumerable<LibGit2Sharp.Commit> parents, Tree tree, CompareOptions compareOptions, Dictionary<string, string> fileOidHolder, Dictionary<string, List<CommittedChange>> pathFilesDic)
        {
            var firstParent = parents.ElementAt(0);
            var secondParent = parents.ElementAt(1);

            var copyOfFileOidHolder1 = new Dictionary<string, string>(fileOidHolder);
            var firstChanges = GetDiffOfTrees(gitRepo, firstParent.Tree, tree, compareOptions, copyOfFileOidHolder1, pathFilesDic);

            var copyOfFileOidHolder2 = new Dictionary<string, string>(fileOidHolder);
            var secondChanges = GetDiffOfTrees(gitRepo, secondParent.Tree, tree, compareOptions, copyOfFileOidHolder2, pathFilesDic);

            var changes = firstChanges.Where(c1 => secondChanges.Any(c2 => c2.Oid == c1.Oid));

            foreach (var change in changes)
            {
                fileOidHolder[change.Path] = copyOfFileOidHolder1[change.Path];
            }

            return changes;
        }

        private ICollection<CommitBlobBlame> ExtraxtBlames(
            string[] blameLines,
            string blobPath,
            string commitSha, Dictionary<string, string> canonicalPathDic,
            Dictionary<string, Commit> commitsDic)
        {
            var blobBlames = new List<CommitBlobBlame>();

            var developerLineOwnershipDictionary = new Dictionary<string, int>();

            foreach (var blameLine in blameLines)
            {
                if (blameLine.Length < 40)
                {
                    continue;
                }

                var sha = blameLine.Substring(0, 40);
                var commit = commitsDic.GetValueOrDefault(sha);

                if (commit is null)
                {
                    continue;
                }

                var email = commit.AuthorEmail;

                var key = sha + email;

                if (!developerLineOwnershipDictionary.ContainsKey(key))
                {
                    developerLineOwnershipDictionary[key] = 0;
                }

                developerLineOwnershipDictionary[key]++;
            }

            foreach (var developerLineOwnership in developerLineOwnershipDictionary)
            {
                var authorSha = developerLineOwnership.Key.Substring(0, 40);
                var email = developerLineOwnership.Key.Substring(40);

                blobBlames.Add(new CommitBlobBlame()
                {
                    Path = blobPath,
                    AuditedLines = developerLineOwnership.Value,
                    DeveloperIdentity = email,
                    AuditedPercentage = developerLineOwnership.Value / (double)blameLines.Length,
                    CommitSha = commitSha,
                    AuthorCommitSha = authorSha,
                    CanonicalPath = canonicalPathDic.GetValueOrDefault(blobPath)
                });
            }

            return blobBlames;
        }

        private string[] GetBlameFromPowerShell(PowerShell powerShellInstance, string commitSha, string blobPath)
        {
            powerShellInstance.Commands.Clear();
            powerShellInstance.AddScript($@"git blame -l -w {commitSha} -- '{blobPath}'");
            return powerShellInstance.Invoke().Select(m => m.ToString()).ToArray();
        }

        private PowerShell GetPowerShellInstance()
        {
            var powerShellInstance = PowerShell.Create();

            powerShellInstance.AddScript($@"set-location '{_localClonePath}'").Invoke();

            return powerShellInstance;
        }

        private List<string> GetBlobsPathFromCommitTree(Tree tree, string[] validExtensions, string[] excludedPaths)
        {
            var result = new List<string>();

            var blobs = tree.Where(m => validExtensions.Any(f => m.Name.EndsWith(f)) && excludedPaths.All(e => !Regex.IsMatch(m.Path, e)));

            result.AddRange(blobs.Select(m => m.Path).ToArray());

            foreach (var treeEntry in tree.Where(m => m.TargetType == TreeEntryTargetType.Tree))
            {
                result.AddRange(GetBlobsPathFromCommitTree((Tree)treeEntry.Target, validExtensions, excludedPaths));
            }

            return result;
        }

        private List<CommittedChange> GetDiffOfTrees(LibGit2Sharp.Repository repo, Tree oldTree,
            Tree newTree, CompareOptions compareOptions, Dictionary<string, string> fileOidHolder, Dictionary<string, List<CommittedChange>> pathFilesDic)
        {
            var result = new List<CommittedChange>();

            foreach (TreeEntryChanges change in repo.Diff.Compare<TreeChanges>(oldTree, newTree, compareOptions))
            {
                var changeStatus = change.Status;

                if (changeStatus == ChangeKind.Unmodified)
                {
                    continue;
                }

                string canonicalPath;

                // 1) files with same path should have same canonical path.
                // 2) files with same path and oldPath should have same canocical path.
                // file ab is created, file ab gets deleted, file a.b is created, file a.b gets renamed to ab. Then the canonical of a.b should be ab.
                if (fileOidHolder.ContainsKey(change.Path))
                {
                    canonicalPath = fileOidHolder[change.Path];
                }
                else if (change.OldExists && changeStatus != ChangeKind.Added)
                {
                    canonicalPath = fileOidHolder[change.OldPath];
                    fileOidHolder[change.Path] = canonicalPath;
                }
                else
                {
                    canonicalPath = fileOidHolder[change.Path] = change.Path;
                }

                var committedFile = new CommittedChange
                {
                    CanonicalPath = canonicalPath, // we fill it later.
                    Status = (short)changeStatus,
                    Oid = change.Oid.Sha,
                    OldPath = change.OldPath,
                    Path = change.Path,
                    OldOid = change.OldOid.Sha,
                    Extension = FileUtility.GetExtension(canonicalPath),
                    FileType = FileUtility.GetFileType(canonicalPath),
                    IsTest = FileUtility.IsTestFile(canonicalPath)
                };

                result.Add(committedFile);
            }

            return result;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
