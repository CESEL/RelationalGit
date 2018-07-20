using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using LibGit2Sharp;
using LibGit2Sharp.Core;
using Microsoft.Extensions.Logging;
using Octokit.Extensions;
using Octokit;
using Octokit.Internal;
using Polly;

namespace RelationalGit
{
    public class GithubDataFetcher
    {
        private readonly GitHubClient _client;
        private readonly string _agentName;
        private readonly string _token;

        public GithubDataFetcher(string token, string agentName)
        {
            Ensure.ArgumentNotNullOrEmptyString(token, nameof(token));
            Ensure.ArgumentNotNullOrEmptyString(token, nameof(agentName));

            _token = token;
            _agentName = agentName;

            var loggerFactory = new LoggerFactory();
            loggerFactory.AddConsole().AddDebug();
            var logger = loggerFactory.CreateLogger("Github.Octokit");

            _client = new ResilientGitHubClientFactory(logger)
                .Create(new ProductHeaderValue(agentName), new Octokit.Credentials(token),new InMemoryCacheProvider());
        }

        internal async Task<GitHubCommit> GetCommit(string owner, string repo, string commitSha)
        {
            return await _client
                    .Repository
                    .Commit
                    .Get(owner,repo, commitSha);       
        }

        internal async Task<IssueEvent[]> GetIssueEvents(string owner, string repo, Issue[] loadedIssues)
        {
            var githubEvents = new List<IssueEvent>();

            for(int i = 0; i < loadedIssues.Length; i++)
            {
                var issueEvents = (await _client
                .Issue
                .Events
                .GetAllForIssue(owner, repo, loadedIssues[i].Number))
                .ToArray();

                var mappedEvents = Mapper.Map<IssueEvent[]>(issueEvents);

                foreach (var mappedEvent in mappedEvents)
                    mappedEvent.IssueNumber = loadedIssues[i].Number;

                githubEvents.AddRange(mappedEvents);
            }

            return githubEvents.ToArray();
        }

        public async Task<PullRequest[]> FetchAllPullRequests(string owner, string repo,string branch = "master")
        {
            Ensure.ArgumentNotNullOrEmptyString(owner, nameof(owner));
            Ensure.ArgumentNotNullOrEmptyString(repo, nameof(repo));
            Ensure.ArgumentNotNullOrEmptyString(branch, nameof(branch));

            var requestFilter = new PullRequestRequest()
            {
                State = ItemStateFilter.Closed,
                SortDirection = SortDirection.Ascending,
                SortProperty = PullRequestSort.Created,
                Base = branch,
            };

            var gitHubPullRequests = (await _client
                .PullRequest
                .GetAllForRepository(owner, repo, requestFilter, new ApiOptions()
                {
                    PageSize=1000
                }))
                .ToArray();

            var pullRequests = Mapper.Map<PullRequest[]>(gitHubPullRequests);
            return pullRequests;
        }

        internal async Task<PullRequestReviewer[]> FetchReviewersOfPullRequests(string owner, string repo,PullRequest[] pullRequests)
        {
            var allReviewers = new List<PullRequestReviewer>();

            for (int i = 0; i < pullRequests.Length; i++)
            {
                allReviewers.AddRange(await GetReviewsOfPullRequest(owner, repo, pullRequests[i]));
            }

            return allReviewers.ToArray();
        }

        private async Task<PullRequestReviewer[]> GetReviewsOfPullRequest(string owner, string repo,PullRequest pullRequest)
        {
            var githubReviews = (await _client
                    .PullRequest
                    .Review
                    .GetAll(owner, repo, pullRequest.Number, new ApiOptions(){PageSize = 1000}))
                    .ToArray();

            var reviews = Mapper.Map<PullRequestReviewer[]>(githubReviews);

            foreach (var review in reviews)
                review.PullRequestNumber = pullRequest.Number;

            return reviews;
        }

        public async Task<Issue[]> GetIssues(string owner, string repo,string[] labels,string state)
        {
            var repositoryIssueRequest = new RepositoryIssueRequest()
            {
                State= (ItemStateFilter)Enum.Parse(typeof(ItemStateFilter), state),
            };

            foreach (var label in labels)
            {
                repositoryIssueRequest.Labels.Add(label);
            }
            
            var githubIssues = (await _client
                    .Issue
                    .GetAllForRepository(owner, repo, repositoryIssueRequest,new ApiOptions()
                    {
                        PageSize=500
                    }));

            var issues = Mapper.Map<Issue[]>(githubIssues);

            return issues;
        }

        internal async Task<PullRequestReviewerComment[]> FetchReviewerCommentsFromRepository(string owner, string repo)
        {
            var requestFilter = new PullRequestReviewCommentRequest()
            {
                Sort = PullRequestReviewCommentSort.Updated,
                Direction= SortDirection.Ascending
            };

            var reviewComments= await _client
                    .PullRequest
                    .ReviewComment
                    .GetAllForRepository(owner, repo, requestFilter, new ApiOptions()
                    {
                        PageSize = 1000
                    });

            var reviewerComments = Mapper.Map<PullRequestReviewerComment[]>(reviewComments.ToArray());
            return reviewerComments;

        }
        internal async Task MergeEvents(string owner, string repo, PullRequest[] pullRequests)
        {
            var reviewerComments = new List<PullRequestReviewerComment>();

            for (int i = 0; i < pullRequests.Length; i++)
            {
                var mergeEvent = (await _client
                          .Issue
                          .Events
                          .GetAllForIssue(owner, repo, pullRequests[i].Number, new ApiOptions()
                          {
                              PageSize = 1000
                          }))
                          .ToArray()
                          .SingleOrDefault(m => m.Event.StringValue == "merged");

                if (mergeEvent != null)
                    pullRequests[i].MergeCommitSha = mergeEvent.CommitId;
                else
                    pullRequests[i].MergeCommitSha = null;
            }
        }
        internal async Task GetUsers(User[] users)
        {
            for (int i = 0; i < users.Length; i++)
            {
                var user = await _client
                    .User
                    .Get(users[i].UserLogin);

                users[i].Email = user.Email;
                users[i].Name = user.Name;
            }
        }
        internal async Task<PullRequestFile[]> FetchFilesOfPullRequests(string owner, string repo,PullRequest[] pullRequests)
        {
            var result = new List<PullRequestFile>();

            for (int i = 0; i < pullRequests.Length; i++)
            {
                var loadedFiles = await GetPullRequestsFiles(owner, repo, pullRequests[i]);

                var mappedFiles = Mapper.Map<PullRequestFile[]>(loadedFiles);

                foreach (var mappedFile in mappedFiles)
                    mappedFile.PullRequestNumber = pullRequests[i].Number;

                result.AddRange(mappedFiles);
            }

            return result.ToArray();
        }

        internal async Task<PullRequestFile[]> GetCommitsOfPullRequests(string owner, string repo, PullRequest[] pullRequests)
        {
            var result = new List<PullRequestFile>();

            for (int i = 0; i < pullRequests.Length; i++)
            {
                if (i % 100 == 0)
                    Console.WriteLine(i + " - " + DateTime.Now);

                var commits = await GetPullRequestsCommits(owner, repo, pullRequests[i]);

                foreach (var commit in commits)
                {
                    /*result.Add(new Models.PullRequestFile()
                    {
                        Id = Guid.NewGuid(),
                        Oid = file.Sha,
                        Additions = file.Additions,
                        Changes = file.Changes,
                        Deletions = file.Changes,
                        FileName = file.FileName,
                        PullRequestNumber = pullRequests[i].Number,
                        Status = file.Status
                    });*/
                }
            }

            return result.ToArray();
        }

        private async Task<IReadOnlyList<Octokit.PullRequestFile>> GetPullRequestsFiles(string owner, string repo,PullRequest pullRequest)
        {
            return await (_client
                .PullRequest
                .Files(owner, repo, pullRequest.Number));
        }

        private async Task<IReadOnlyList<Octokit.PullRequestCommit>> GetPullRequestsCommits(string owner, string repo, PullRequest pullRequest)
        {
            return await (_client
                    .PullRequest
                    .Commits(owner, repo, pullRequest.Number));
        }
    }
}
