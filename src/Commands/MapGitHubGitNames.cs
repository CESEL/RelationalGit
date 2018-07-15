
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Diacritics.Extensions;
using F23.StringSimilarity;

namespace RelationalGit.Commands
{
    public class MapGitHubGitNamesCommand
    {
        public async Task Execute(string token, string agenName, string owner, string repo)
        {
            using (var dbContext = new GitRepositoryDbContext())
            {
                var commitAuthors= dbContext.Commits
                .FromSql(@"select c1.* from Commits as c1
                INNER JOIN (select NormalizedAuthorName,AuthorName,max(AuthorDateTime) as AuthorDateTime from Commits
                group by NormalizedAuthorName,AuthorName) as c2 on c1.AuthorDateTime=c2.AuthorDateTime
                and c1.NormalizedAuthorName=c2.NormalizedAuthorName
                and c1.AuthorName=c2.AuthorName")
                .ToArray();

                var github = new GithubDataFetcher(token, agenName);

                foreach (var commitAuthor in commitAuthors)
                {
                    var commit = await github.GetCommit(owner,repo,commitAuthor.Sha);

                    // Github does not return the author for some of the old Commits
                    dbContext.Add(new GitHubGitUser{
                        GitUsername =  commitAuthor.AuthorName,
                        GitHubUsername = commit.Author?.Login,
                        GitNormalizedUsername = commitAuthor.NormalizedAuthorName
                    });
                }

                await dbContext.SaveChangesAsync();
            }
        }
    }
}
