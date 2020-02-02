using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace RelationalGit.Commands
{
    public class GetGitBlobsAndTheirBlamesOfCommitCommand
    {
        private readonly ILogger _logger;

        public GetGitBlobsAndTheirBlamesOfCommitCommand(ILogger logger)
        {
            _logger = logger;
        }

        public async Task Execute(string repoPath, string branchName, string commitSha, string[] validExtensions, string[] excludedBlamePaths)
        {
            /*
            using (var dbContext = new GitRepositoryDbContext(false))
            {
                
                _logger.LogInformation("{datetime}: Extracting Blames of {Commitsha}", DateTime.Now, commitSha);

                var canonicalDic = dbContext.GetCanonicalPaths();

                var gitRepository = new GitRepository(repoPath, _logger);
                var orderedCommits = gitRepository.ExtractCommitsFromBranch(branchName);

                var commit = orderedCommits.Single(q => q.Sha == commitSha);
                gitRepository.LoadBlobsAndTheirBlamesOfCommit(commit, validExtensions, excludedBlamePaths, canonicalDic);

                dbContext.CommittedBlob.AddRange(commit.Blobs);
                dbContext.CommitBlobBlames.AddRange(commit.Blobs.SelectMany(m => m.CommitBlobBlames));

                await dbContext.SaveChangesAsync();
            
             }*/
        }
    }
}
