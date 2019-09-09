
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RelationalGit.Data;
using RelationalGit.Gathering.GitHub;

namespace RelationalGit.Commands
{
    public class MapGitHubGitNamesCommand
    {
        private readonly ILogger _logger;

        public MapGitHubGitNamesCommand(ILogger logger)
        {
            _logger = logger;
        }

        public async Task Execute(string token, string agenName, string owner, string repo)
        {
            using (var dbContext = new GitRepositoryDbContext())
            {
                var commitAuthors = await dbContext.Commits
                .FromSql(@"select c1.* from Commits as c1
                INNER JOIN (select NormalizedAuthorName,AuthorName,max(AuthorDateTime) as AuthorDateTime from Commits
                group by NormalizedAuthorName,AuthorName) as c2 on c1.AuthorDateTime=c2.AuthorDateTime
                and c1.NormalizedAuthorName=c2.NormalizedAuthorName
                and c1.AuthorName=c2.AuthorName")
                .ToArrayAsync().ConfigureAwait(false);

                _logger.LogInformation("{datetime}: fetching corresponding GitHub user of {count} git authors.", DateTime.Now, commitAuthors.Length);

                var github = new GithubDataFetcher(token, agenName, _logger);

                foreach (var commitAuthor in commitAuthors)
                {
                    var commit = await github.GetCommit(owner, repo, commitAuthor.Sha).ConfigureAwait(false);

                    // Github does not return the author for some of the old Commits
                    dbContext.Add(new GitHubGitUser
                    {
                        GitUsername = commitAuthor.AuthorName,
                        GitHubUsername = commit.Author?.Login,
                        GitNormalizedUsername = commitAuthor.NormalizedAuthorName
                    });
                }

                await dbContext.SaveChangesAsync().ConfigureAwait(false);

                _logger.LogInformation("{datetime}: corresponding GitHub users have been saved successfully.", DateTime.Now);
            }
        }
    }
}
