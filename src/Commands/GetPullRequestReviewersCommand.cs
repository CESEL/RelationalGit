using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationalGit.Commands
{
    public class GetPullRequestReviewersCommand
    {
        public async Task Execute(string token, string agenName, string owner, string repo, string branch)
        {
            using (var dbContext = new GitRepositoryDbContext(false))
            {
                var loadedPullRequests = dbContext.PullRequests.AsNoTracking()
                    .OrderBy(q => q.Number)
                    .ToArray();

                //dbContext.Database.ExecuteSqlCommand($"TRUNCATE TABLE PullRequestReviewers");
                var githubExtractor = new GithubDataFetcher(token, agenName);
                var pullRequestReviews = await githubExtractor.FetchReviewersOfPullRequests(owner, repo, loadedPullRequests);

                dbContext.AddRange(pullRequestReviews);
                dbContext.SaveChanges();
            }
        }
    }
}
