
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
using Microsoft.Extensions.Logging;

namespace RelationalGit
{
    public class GitRepository
    {
        #region Fields
        private static readonly Regex _blameRegex = new Regex(@"\w{40}\t\(<(?<email>.*)>\t",RegexOptions.Compiled);
        private Repository _gitRepo;
        private string _localClonePath;
        private Dictionary<string, string> _fileOidHolder = new Dictionary<string, string>();
        private ILogger _logger;

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
            Ensure.ArgumentNotNullOrEmptyString(branchName,nameof(branchName));

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

            var commits = _gitRepo
                .Commits
                .QueryBy(filter)
                .ToArray();
            
            var extractedCommits = Mapper.Map<Commit[]>(commits);

            return extractedCommits;
        }

        public void LoadChangesOfCommits(Commit[] orderedCommits)
        {
            _fileOidHolder.Clear();

            _logger.LogInformation("{datetime}: trying to get committed changes from all the {count} commits.", DateTime.Now,orderedCommits.Count());

            for (int i = 0; i < orderedCommits.Length; i++)
            {
                if(i%500==0)
                    _logger.LogInformation("{dateTime}: more than {count} commits has been processed",DateTime.Now,i);

                LoadChangesOfCommit(orderedCommits[i]);
            }
        }

        public void LoadBlobsAndTheirBlamesOfCommit(Commit commit, string[] validExtensions, Dictionary<string, string> canonicalPathDic, string branchName = "master")
        {
            var blobsPath = GetBlobsPathFromCommitTree(commit.GitCommit.Tree, validExtensions); 
            
            var committedBlobs = new ConcurrentBag<CommittedBlob>();

            Parallel.ForEach(
                blobsPath,
                new ParallelOptions() {MaxDegreeOfParallelism=6},
                () =>
                {
                    return  GetPowerShellInstance();
                },
                (blobPath, i, powerShellInstance) =>
                {
                    var blobBlames = GetBlobBlamesOfCommit(powerShellInstance, commit.Sha, blobPath, canonicalPathDic);

                    committedBlobs.Add(new CommittedBlob()
                    {
                        NumberOfLines = blobBlames.Sum(m => m.AuditedLines),
                        Path = blobPath,
                        CommitSha = commit.Sha,
                        CanonicalPath = canonicalPathDic.GetValueOrDefault(blobPath),
                        CommitBlobBlames = blobBlames
                    });

                    if(committedBlobs.Count()%500==0)
                        _logger.LogInformation("{datetime} : Extracted Blames {currentCount} out of {total}"
                        ,DateTime.Now,committedBlobs.Count(),blobsPath.Count);


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

            var blobBlames = ExtraxtBlames(rawBlameLines,blobPath,commitSha,canonicalPathDic);

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
                    RenameDetectionMode = RenameDetectionMode.Renames,
                }
            };

            if (gitCommit.Parents.Count() <= 1)
                committedChanges = GetDiffOfTrees(_gitRepo, gitCommit.Parents.SingleOrDefault()?.Tree, gitCommit.Tree, compareOptions);
            else
            {
                committedChanges.AddRange(GetDiffOfMergedTrees(_gitRepo, gitCommit.Parents, gitCommit.Tree, compareOptions));
            }

            foreach (var committedFile in committedChanges)
                committedFile.CommitSha = commit.Sha;

            commit.CommittedChanges = committedChanges;
        }

        private IEnumerable<CommittedChange> GetDiffOfMergedTrees(Repository gitRepo, IEnumerable<LibGit2Sharp.Commit> parents, Tree tree, CompareOptions compareOptions)
        {
            var firstParent = parents.ElementAt(0);
            var secondParent = parents.ElementAt(1);

            var firstChanges = GetDiffOfTrees(gitRepo,firstParent.Tree,tree,compareOptions);
            var secondChanges = GetDiffOfTrees(gitRepo, secondParent.Tree, tree, compareOptions);

            var result = firstChanges.Where(c1 => secondChanges.Any(c2 => c2.Oid==c1.Oid));
            return result;
        }

        private ICollection<CommitBlobBlame> ExtraxtBlames(string[] blameLines,string blobPath,string commitSha,Dictionary<string, string> canonicalPathDic)
        {

            var blobBlames = new List<CommitBlobBlame>();

            var developerLineOwnershipDictionary = new Dictionary<string, int>();

            foreach (var blameLine in blameLines)
            {
                var mc = _blameRegex.Matches(blameLine);
                if (mc.Count == 0)
                    continue;

                var email = mc[0].Groups["email"].Value;

                var sha=blameLine.Substring(0,40);
                var key=sha+email;

                if (!developerLineOwnershipDictionary.ContainsKey(key))
                    developerLineOwnershipDictionary[key] = 0;

                developerLineOwnershipDictionary[key]++;
            }

            foreach (var developerLineOwnership in developerLineOwnershipDictionary)
            {
                var authorSha=developerLineOwnership.Key.Substring(0,40);
                var email=developerLineOwnership.Key.Substring(40);

                blobBlames.Add(new CommitBlobBlame()
                {
                    Path=blobPath,
                    AuditedLines = developerLineOwnership.Value,
                    DeveloperIdentity = email,
                    AuditedPercentage = developerLineOwnership.Value / (double)blameLines.Length,
                    CommitSha = commitSha,
                    AuthorCommitSha=authorSha,
                    CanonicalPath = canonicalPathDic.GetValueOrDefault(blobPath)
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

            powerShellInstance
                .AddScript($@"set-location '{_localClonePath}'")
                .Invoke();
            
            return powerShellInstance;
        }

        private List<string> GetBlobsPathFromCommitTree(Tree tree,string[] validExtensions)
        {
            var result = new List<string>();

            var blobs = tree.Where(m => m.TargetType == TreeEntryTargetType.Blob
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

                if (changeStatus == ChangeKind.Unmodified)
                    continue;

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
                    OldOid = change.OldOid.Sha,
                    Extension=FileUtility.GetExtension(canonicalPath),
                    FileType=FileUtility.GetFileType(canonicalPath),
                    IsTest=FileUtility.IsTestFile(canonicalPath)
                };

                result.Add(committedFile);
            }

            return result;
        }
    }
}
