using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationalGit.Commands
{
    public class GetPullRequestReviewerCommentsCommand
    {
        public async Task Execute(string token, string agenName, string owner, string repo, string branch)
        {
            using (var dbContext = new GitRepositoryDbContext())
            {
                dbContext.Database.ExecuteSqlCommand($"TRUNCATE TABLE PullRequestReviewerComments");
                var githubExtractor = new GithubDataFetcher(token, agenName);
                var pullRequestReviewerComments = await githubExtractor.FetchReviewerCommentsFromRepository(owner, repo);

                dbContext.AddRange(pullRequestReviewerComments);
                dbContext.SaveChanges();
            }
        }
    }
}
