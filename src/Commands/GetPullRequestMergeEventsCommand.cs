using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationalGit.Commands
{
    public class GetPullRequestMergeEventsCommand
    {
        public async Task Execute(string token, string agenName, string owner, string repo, string branch)
        {
            using (var dbContext = new GitRepositoryDbContext())
            {
                var loadedPullRequests = dbContext.PullRequests
                    .FromSql(@"select * from PullRequests
                            WHERE MergeCommitSha not in (select Sha from Commits)")
                            .ToArray();

                var githubExtractor = new GithubDataFetcher(token, agenName);
                await githubExtractor.MergeEvents(owner, repo, loadedPullRequests);

                dbContext.SaveChanges();
            }
        }
    }
}
