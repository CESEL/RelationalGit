using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationalGit.Commands
{
    public class GetGitBlobsAndTheirBlamesOfCommitCommand
    {
        private readonly ILogger _logger;

        public GetGitBlobsAndTheirBlamesOfCommitCommand(ILogger logger)
        {
            _logger=logger;
        }
        public async Task Execute(string repoPath,string branchName,string commitSha,string[] validExtensions)
        {
            using (var dbContext = new GitRepositoryDbContext(false))
            {
                _logger.LogInformation("{datetime}: Extracting Blames of {Commitsha}",DateTime.Now,commitSha);

                var canonicalDic = dbContext.GetCanonicalPaths();

                var gitRepository = new GitRepository(repoPath,_logger);
                var orderedCommits = gitRepository.ExtractCommitsFromBranch(branchName);

                var commit = orderedCommits.Single(q => q.Sha == commitSha);
                gitRepository.LoadBlobsAndTheirBlamesOfCommit(commit, validExtensions, canonicalDic);

                dbContext.CommittedBlob.AddRange(commit.Blobs);
                dbContext.CommitBlobBlames.AddRange(commit.Blobs.SelectMany(m => m.CommitBlobBlames));

                await dbContext.SaveChangesAsync();
            }
        }
    }
}
