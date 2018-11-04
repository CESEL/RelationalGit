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
        private readonly ILogger _logger;
        private readonly string _token;

        public GithubDataFetcher(string token, string agentName, ILogger logger)
        {
            Ensure.ArgumentNotNullOrEmptyString(token, nameof(token));
            Ensure.ArgumentNotNullOrEmptyString(token, nameof(agentName));

            _token = token;
            _agentName = agentName;

            _logger = logger;

            _client = new ResilientGitHubClientFactory(_logger)
                .Create(new ProductHeaderValue(agentName), new Octokit.Credentials(token),new InMemoryCacheProvider());
        }

        internal async Task<IEnumerable<Commit>> FetchCommits(string owner, string repo, string authorGitHubLogin)
        {
            Ensure.ArgumentNotNullOrEmptyString(owner, nameof(owner));
            Ensure.ArgumentNotNullOrEmptyString(repo, nameof(repo));
            Ensure.ArgumentNotNullOrEmptyString(repo, nameof(authorGitHubLogin));

            _logger.LogInformation("{datetime}: fetching comments of {authorGitHubLogin} from {owner}/{repo}.", DateTime.Now, authorGitHubLogin, owner, repo);

           var result = (await _client.Repository.Commit.GetAll(owner, repo, new CommitRequest {Author= authorGitHubLogin }, new ApiOptions() { PageSize = 1000 })).ToArray();

            _logger.LogInformation("{datetime}: {count} commits have been fetched.", DateTime.Now, result.Length);

            return null;

        }

        internal async Task<IEnumerable<IssueComment>> FetchPullRequestIssueCommentsFromRepository(string owner, string repo,PullRequest[] pullRequests)
        {
            Ensure.ArgumentNotNullOrEmptyString(owner, nameof(owner));
            Ensure.ArgumentNotNullOrEmptyString(repo, nameof(repo));

            _logger.LogInformation("{datetime}: fetching all the issue comments from {owner}/{repo}.", DateTime.Now, owner, repo);

            var requestFilter = new IssueCommentRequest()
            {
                Sort = IssueCommentSort.Updated,
                Direction = SortDirection.Ascending
            };

            var allComments = new List<IssueComment>();

            foreach (var pullRequest in pullRequests)
            {
                var issueComments = await _client.Issue.Comment.GetAllForIssue(owner, repo,pullRequest.Number, new ApiOptions() { PageSize = 1000 });
                allComments.AddRange(Mapper.Map<IssueComment[]>(issueComments.ToArray()));
            }

            _logger.LogInformation("{datetime}: {count} issue comments have been fetched.", DateTime.Now, allComments.Count);

            return allComments;

        }

        public async Task<GitHubCommit> GetCommit(string owner, string repo, string commitSha)
        {
            Ensure.ArgumentNotNullOrEmptyString(owner, nameof(owner));
            Ensure.ArgumentNotNullOrEmptyString(repo, nameof(repo));
            Ensure.ArgumentNotNullOrEmptyString(repo, nameof(commitSha));

            var commit = await _client.Repository.Commit.Get(owner,repo, commitSha);
            _logger.LogInformation("{datetime}: commit {commit} has been fetched successfully.", DateTime.Now, commitSha);
            return commit;

        }

        public async Task<IssueEvent[]> GetIssueEvents(string owner, string repo, Issue[] loadedIssues)
        {
            Ensure.ArgumentNotNullOrEmptyString(owner, nameof(owner));
            Ensure.ArgumentNotNullOrEmptyString(repo, nameof(repo));
            Ensure.ArgumentNotNull(loadedIssues, nameof(loadedIssues));

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

        public async Task<PullRequest[]> FetchAllPullRequests(string owner, string repo,string branch)
        {

            Ensure.ArgumentNotNullOrEmptyString(owner, nameof(owner));
            Ensure.ArgumentNotNullOrEmptyString(repo, nameof(repo));
            

            _logger.LogInformation("{datetime}: fetching pull reuqests from {owner}/{repo}.", DateTime.Now, owner,repo);

            var requestFilter = new PullRequestRequest()
            {
                State = ItemStateFilter.Closed,
                SortDirection = SortDirection.Ascending,
                SortProperty = PullRequestSort.Created,
                Base = branch,
            };

            var gitHubPullRequests = (await _client.PullRequest.GetAllForRepository(owner, repo, requestFilter, new ApiOptions()
            {
                PageSize=1000
            }))
            .ToArray();

            var pullRequests = Mapper.Map<PullRequest[]>(gitHubPullRequests);

            _logger.LogInformation("{datetime}: {count} pull requests have been fetched.", DateTime.Now, pullRequests.Length);

            return pullRequests;
        }

        public async Task<PullRequestReviewer[]> FetchReviewersOfPullRequests(string owner, string repo,PullRequest[] pullRequests)
        {
            Ensure.ArgumentNotNullOrEmptyString(owner, nameof(owner));
            Ensure.ArgumentNotNullOrEmptyString(repo, nameof(repo));
            Ensure.ArgumentNotNull(pullRequests,nameof(pullRequests));

            _logger.LogInformation("{datetime}: fetching all the pull reuqest reviewers from {owner}/{repo}.", DateTime.Now, owner, repo);

            var allReviewers = new List<PullRequestReviewer>();

            for (int i = 0; i < pullRequests.Length; i++)
            {
                allReviewers.AddRange(await GetReviewsOfPullRequest(owner, repo, pullRequests[i]));
            }

            _logger.LogInformation("{datetime}: {count} reviewers have been fetched.", DateTime.Now, allReviewers.Count);

            return allReviewers.ToArray();
        }

        public async Task<Issue[]> GetIssues(string owner, string repo,string[] labels,string state)
        {
            Ensure.ArgumentNotNullOrEmptyString(owner, nameof(owner));
            Ensure.ArgumentNotNullOrEmptyString(repo, nameof(repo));

            var repositoryIssueRequest = new RepositoryIssueRequest()
            {
                State= (ItemStateFilter)Enum.Parse(typeof(ItemStateFilter), state),
            };

            foreach (var label in labels)
            {
                repositoryIssueRequest.Labels.Add(label);
            }
            
            var githubIssues = (await _client.Issue.GetAllForRepository(owner, repo, repositoryIssueRequest,new ApiOptions()
            {
                PageSize=500
            }));

            var issues = Mapper.Map<Issue[]>(githubIssues);

            return issues;
        }

        public async Task<PullRequestReviewerComment[]> FetchPullRequestReviewerCommentsFromRepository(string owner, string repo)
        {

            Ensure.ArgumentNotNullOrEmptyString(owner, nameof(owner));
            Ensure.ArgumentNotNullOrEmptyString(repo, nameof(repo));

            var requestFilter = new PullRequestReviewCommentRequest()
            {
                Sort = PullRequestReviewCommentSort.Updated,
                Direction= SortDirection.Ascending
            };

            _logger.LogInformation("{datetime}: fetching review comment of all the pull reuqests from {owner}/{repo}.", DateTime.Now, owner, repo);

            var reviewComments= await _client
                    .PullRequest
                    .ReviewComment
                    .GetAllForRepository(owner, repo, requestFilter, new ApiOptions()
                    {
                        PageSize = 1000
                    });

            var reviewerComments = Mapper.Map<PullRequestReviewerComment[]>(reviewComments.ToArray());

            _logger.LogInformation("{datetime}: {count} review comments have been fetched.", DateTime.Now, reviewerComments.Length);

            return reviewerComments;

        }
        public async Task MergeEvents(string owner, string repo, PullRequest[] pullRequests)
        {
            Ensure.ArgumentNotNullOrEmptyString(owner, nameof(owner));
            Ensure.ArgumentNotNullOrEmptyString(repo, nameof(repo));
            Ensure.ArgumentNotNull(pullRequests, nameof(pullRequests));

            var reviewerComments = new List<PullRequestReviewerComment>();

            _logger.LogInformation("{datetime}: fetching merged events of {count} pull reuqests of {owner}/{repo}.", DateTime.Now, pullRequests.Length, owner, repo);

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

                if(i%500==0)
                    _logger.LogInformation("{datetime}: more than {count} pull requests has been processed.", DateTime.Now, i);
            }
        }
        public async Task GetUsers(User[] users)
        {
            Ensure.ArgumentNotNull(users, nameof(users));

            for (int i = 0; i < users.Length; i++)
            {
                var user = await _client
                    .User
                    .Get(users[i].UserLogin);

                users[i].Email = user.Email;
                users[i].Name = user.Name;
            }
        }
        public async Task<PullRequestFile[]> FetchFilesOfPullRequests(string owner, string repo,PullRequest[] pullRequests)
        {
            Ensure.ArgumentNotNullOrEmptyString(owner, nameof(owner));
            Ensure.ArgumentNotNullOrEmptyString(repo, nameof(repo));
            Ensure.ArgumentNotNull(pullRequests, nameof(pullRequests));

            _logger.LogInformation("{datetime}: fetching files of all the {count} pull reuqests of {owner}/{repo}.", DateTime.Now, pullRequests.Length, owner, repo);

            var result = new List<PullRequestFile>();

            for (int i = 0; i < pullRequests.Length; i++)
            {
                var loadedFiles = await GetPullRequestsFiles(owner, repo, pullRequests[i]);

                var mappedFiles = Mapper.Map<PullRequestFile[]>(loadedFiles);

                foreach (var mappedFile in mappedFiles)
                    mappedFile.PullRequestNumber = pullRequests[i].Number;

                result.AddRange(mappedFiles);
            }

            _logger.LogInformation("{datetime}: {count} pull request files haved been fetched.", DateTime.Now, result.Count);

            return result.ToArray();
        }
        public async Task<PullRequestFile[]> GetCommitsOfPullRequests(string owner, string repo, PullRequest[] pullRequests)
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

        private async Task<PullRequestReviewer[]> GetReviewsOfPullRequest(string owner, string repo, PullRequest pullRequest)
        {
            var githubReviews = (await _client
                    .PullRequest
                    .Review
                    .GetAll(owner, repo, pullRequest.Number, new ApiOptions() { PageSize = 1000 }))
                    .ToArray();

            var reviews = Mapper.Map<PullRequestReviewer[]>(githubReviews);

            foreach (var review in reviews)
                review.PullRequestNumber = pullRequest.Number;

            return reviews;
        }
    }
}
