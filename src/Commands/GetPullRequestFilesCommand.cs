using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RelationalGit.Commands
{
    public class GetPullRequestFilesCommand
    {
        public async Task Execute(string token, string agenName, string owner, string repo, string branch)
        {
            using (var dbContext = new GitRepositoryDbContext())
            {
                var loadedPullRequests = await dbContext.PullRequests.ToArrayAsync();

                var githubExtractor = new GithubDataFetcher(token, agenName);
                var files = await githubExtractor.FetchFilesOfPullRequests(owner, repo, loadedPullRequests);

                dbContext.AddRange(files);
                dbContext.SaveChanges();
            }
        }
    }
}
