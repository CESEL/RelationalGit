using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RelationalGit.Data;
using RelationalGit.Gathering.Git;

namespace RelationalGit.Commands
{
    public class GetGitBlobsAndTheirBlamesForPeriodsCommand
    {
        private readonly ILogger _logger;

        public GetGitBlobsAndTheirBlamesForPeriodsCommand(ILogger logger)
        {
            _logger = logger;
        }

        public async Task Execute(ExtractBlameForEachPeriodOption options)
        {
            var dbContext = new GitRepositoryDbContext();

            var extractedCommits = await dbContext.CommitBlobBlames.Select(m => m.CommitSha).Distinct().ToArrayAsync().ConfigureAwait(false);
            var periods = await GetPeriods(options, dbContext, extractedCommits).ConfigureAwait(false);

            var canonicalDic = dbContext.GetCanonicalPaths();

            var gitRepository = new GitRepository(options.RepositoryPath, _logger);
            var orderedCommits = gitRepository.ExtractCommitsFromBranch(options.GitBranch).ToDictionary(q => q.Sha);

            dbContext.Dispose();

            _logger.LogInformation("{datetime}: extracting blames for {count} periods.", DateTime.Now, periods.Count());

            foreach (var period in periods)
            {
                await ExtractBlamesofCommit(orderedCommits[period.LastCommitSha], canonicalDic, options.Extensions, options.ExcludedBlamePaths, gitRepository, options.GitBranch, options.ExtractBlames).ConfigureAwait(false);
            }
        }

        private static async Task<Period[]> GetPeriods(ExtractBlameForEachPeriodOption options, GitRepositoryDbContext dbContext, string[] extractedCommits)
        {
            var periods = await dbContext.Periods.OrderBy(q => q.ToDateTime).ToArrayAsync().ConfigureAwait(false);

            periods = periods.Where(m => !extractedCommits.Any(c => c == m.LastCommitSha)).ToArray();

            if (options.PeriodIds != null && options.PeriodIds.Count() > 0)
            {
                periods = periods.Where(q => options.PeriodIds.Any(p => p == q.Id)).ToArray();
            }

            return periods;
        }

        private async Task ExtractBlamesofCommit(Commit commit, Dictionary<string, string> canonicalDic, string[] validExtensions, string[] excludePath, GitRepository gitRepository, string branch, bool extractBlames)
        {
            using (var dbContext = new GitRepositoryDbContext(false))
            {
                _logger.LogInformation("{datetime}: extracting blames out of commit {Commitsha}", DateTime.Now, commit.Sha);

                gitRepository.LoadBlobsAndTheirBlamesOfCommit(commit, validExtensions, excludePath, canonicalDic, extractBlames, branch);

                var blames = commit.Blobs.SelectMany(m => m.CommitBlobBlames);
                _logger.LogInformation("{datetime}: saving {count} blames of {blobCount} from commit {Commitsha} into database.", DateTime.Now, blames.Count(), commit.Blobs.Count(), commit.Sha);

                await dbContext.BulkInsertAsync(commit.Blobs.ToArray()).ConfigureAwait(false);
                await dbContext.BulkInsertAsync(blames.ToArray()).ConfigureAwait(false);

                _logger.LogInformation("{datetime}: blames of {Commitsha} have been saved.", DateTime.Now, commit.Sha);
            }
        }
    }
}
