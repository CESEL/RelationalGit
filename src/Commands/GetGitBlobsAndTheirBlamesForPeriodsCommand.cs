using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationalGit.Commands
{
    public class GetGitBlobsAndTheirBlamesForPeriodsCommand
    {
        private readonly ILogger _logger;

        public GetGitBlobsAndTheirBlamesForPeriodsCommand(ILogger logger)
        {
            _logger = logger;
        }
        public async Task Execute(string repoPath, string branchName, string[] validExtensions)
        {
            var dbContext = new GitRepositoryDbContext();
            
            var extractedCommits=dbContext.CommitBlobBlames.Select(m=>m.CommitSha).Distinct()
            .ToArray();

            var periods = await dbContext.Periods.OrderBy(q => q.ToDateTime)
            .ToArrayAsync();

            periods=periods
                .Where(m=>!extractedCommits.Any(c=>c==m.LastCommitSha))
                .ToArray();

            var canonicalDic = dbContext.GetCanonicalPaths();

            var gitRepository = new GitRepository(repoPath);
            var orderedCommits = gitRepository.ExtractCommitsFromBranch(branchName).ToDictionary(q => q.Sha);

            dbContext.Dispose();

            foreach(var period in periods)
                await ExtractBlamesofCommit(orderedCommits[period.LastCommitSha], canonicalDic, validExtensions, gitRepository);
                  
        }
        private async Task ExtractBlamesofCommit(Commit commit, Dictionary<string, string> canonicalDic, string[] validExtensions, GitRepository gitRepository)
        {
            using (var dbContext = new GitRepositoryDbContext(false))
            {
                _logger.LogInformation("{datetime}: extracting blames of {Commitsha}", DateTime.Now, commit.Sha);

                gitRepository.LoadBlobsAndTheirBlamesOfCommit(commit, validExtensions, canonicalDic);

                dbContext.CommittedBlob.AddRange(commit.Blobs);
                dbContext.CommitBlobBlames.AddRange(commit.Blobs.SelectMany(m => m.CommitBlobBlames));
        
                _logger.LogInformation("{datetime}: finished extracting blames of {Commitsha}", DateTime.Now, commit.Sha);

                await dbContext.SaveChangesAsync();

                _logger.LogInformation("{datetime}: blames of {Commitsha} saved.", DateTime.Now, commit.Sha);

            }
        }
    }
}
