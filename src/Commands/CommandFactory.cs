using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RelationalGit.Commands
{
    public class CommandFactory
    {
        public async Task Execute(string[] args)
        {
            var command = args[0];

            await RunCommand(command, args);
        }

        private static async Task RunCommand(string command, string[] args)
        {
            if (command.ToLower() == "-get-pullrequests")
            {
                var cmd = new GetPullRequestsCommand();
                await cmd.Execute(args[1], args[2], args[3], args[4], args[5]);
            }
            else if (command.ToLower() == "-get-pullrequest-reviewers")
            {
                var cmd = new GetPullRequestReviewersCommand();
                await cmd.Execute(args[1], args[2], args[3], args[4], args[5]);
            }
            else if (command.ToLower() == "-get-pullrequest-reviewer-comments")
            {
                var cmd = new GetPullRequestReviewerCommentsCommand();
                await cmd.Execute(args[1], args[2], args[3], args[4], args[5]);
            }
            else if (command.ToLower() == "-get-merge-events")
            {
                var cmd = new GetPullRequestMergeEventsCommand();
                await cmd.Execute(args[1], args[2], args[3], args[4], args[5]);
            }
            else if (command.ToLower() == "-get-users")
            {
                var cmd = new GetUsersCommand();
                await cmd.Execute(args[1], args[2]);
            }
            else if (command.ToLower() == "-get-pullrequests-files")
            {
                var cmd = new GetPullRequestFilesCommand();
                await cmd.Execute(args[1], args[2], args[3], args[4], args[5]);
            }
            else if (command.ToLower() == "-get-git-commits")
            {
                var cmd = new GetGitCommitsCommand();
                await cmd.Execute(args[1], args[2]);
            }
            else if (command.ToLower() == "-get-git-commitschanges")
            {
                var cmd = new GetGitCommitsChangesCommand();
                await cmd.Execute(args[1], args[2]);
            }
            else if (command.ToLower() == "-get-git-blobsblames")
            {
                var cmd = new GetGitBlobsAndTheirBlamesOfCommitCommand();
                await cmd.Execute(args[1], args[2], args[3],args[4].Split(","));
            }
            else if (command.ToLower() == "-get-git-blobsblames-for-periods")
            {
                var cmd = new GetGitBlobsAndTheirBlamesForPeriodsCommand();
                await cmd.Execute(args[1], args[2], args[3].Split(","));
            }
            else if (command.ToLower() == "-periodize-git-commits")
            {
                var cmd = new PeriodizeGitCommits();
                await cmd.Execute(args[1], args[2], 90);
            }
        }
    }
}
