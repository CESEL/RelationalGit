using RelationalGit.CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationalGit.Commands
{
    internal class CommandFactory
    {
        public async Task Execute(InputOption options)
        {
            await RunCommand(options);
        }

        private static async Task RunCommand(InputOption options)
        {
            
            if (options.Command.ToLower() == CommandType.GetPullRequests)
            {
                var cmd = new GetPullRequestsCommand();
                await cmd.Execute(options.GitHubToken, agenName:"mirsaeedi", options.GitHubOwner, options.GitHubRepo, options.GitBranch);
            }
            else if (options.Command.ToLower() ==CommandType.GetPullRequestReviewes)
            {
                var cmd = new GetPullRequestReviewersCommand();
                await cmd.Execute(options.GitHubToken, agenName: "mirsaeedi", options.GitHubOwner, options.GitHubRepo, options.GitBranch);
            }
            else if (options.Command.ToLower() == CommandType.GetPullRequestRevieweComments)
            {
                var cmd = new GetPullRequestReviewerCommentsCommand();
                await cmd.Execute(options.GitHubToken, agenName: "mirsaeedi", options.GitHubOwner, options.GitHubRepo, options.GitBranch);
            }
            else if (options.Command.ToLower() == "-get-merge-events")
            {
                var cmd = new GetPullRequestMergeEventsCommand();
                await cmd.Execute(options.GitHubToken, agenName: "mirsaeedi", options.GitHubOwner, options.GitHubRepo, options.GitBranch);
            }
            else if (options.Command.ToLower() == "-get-users")
            {
                var cmd = new GetUsersCommand();
                await cmd.Execute(options.GitHubToken, agentName: "mirsaeedi");
            }
            else if (options.Command.ToLower() == CommandType.GetPullRequestFiles)
            {
                var cmd = new GetPullRequestFilesCommand();
                await cmd.Execute(options.GitHubToken, agenName: "mirsaeedi", options.GitHubOwner, options.GitHubRepo, options.GitBranch);
            }
            else if (options.Command.ToLower() == "-get-issues")
            {
                var cmd = new GetIssuesCommand();

                await cmd.Execute(options.GitHubToken, agenName: "mirsaeedi", owner: options.GitHubOwner, repo: options.GitHubRepo, 
                    labels:options.IssueLabels.ToArray(), 
                    state: options.IssueState);
            }
            else if (options.Command.ToLower() == "-get-issuesevents")
            {
                var cmd = new GetIssuesEventsCommand();

                await cmd.Execute(options.GitHubToken, agenName: "mirsaeedi",owner: options.GitHubOwner,repo: options.GitHubRepo);
            }
            else if (options.Command.ToLower() == CommandType.GetGitCommits)
            {
                var cmd = new GetGitCommitsCommand();
                await cmd.Execute(options.RepositoryPath, options.GitBranch);
            }
            else if (options.Command.ToLower() == CommandType.GetGitCommitsChanges)
            {
                var cmd = new GetGitCommitsChangesCommand();
                await cmd.Execute(options.RepositoryPath, options.GitBranch);
            }
            else if (options.Command.ToLower() == CommandType.ExtractBlameFromCommit)
            {
                var cmd = new GetGitBlobsAndTheirBlamesOfCommitCommand();
                await cmd.Execute(options.RepositoryPath, options.GitBranch, options.CommitSha,options.Extensions.ToArray());
            }
            else if (options.Command.ToLower() == CommandType.ExtractBlameForEachPeriod)
            {
                var cmd = new GetGitBlobsAndTheirBlamesForPeriodsCommand();
                await cmd.Execute(options.RepositoryPath, options.GitBranch, options.Extensions.ToArray());
            }
            else if (options.Command.ToLower() == CommandType.Periodize)
            {
                var cmd = new PeriodizeGitCommits();
                await cmd.Execute(options.RepositoryPath, options.GitBranch, 90);
            }
        }
    }
}
