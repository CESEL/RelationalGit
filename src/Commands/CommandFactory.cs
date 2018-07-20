using Microsoft.Extensions.Logging;
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
        public async Task Execute(InputOption options,ILogger logger)
        {
            await RunCommand(options,logger);
        }

        private static async Task RunCommand(InputOption options,ILogger logger)
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
            else if (options.Command.ToLower() == CommandType.GetPullRequestMergeEvents)
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
                var cmd = new GetGitBlobsAndTheirBlamesOfCommitCommand(logger);
                await cmd.Execute(options.RepositoryPath, options.GitBranch, options.CommitSha,options.Extensions.ToArray());
            }
            else if (options.Command.ToLower() == CommandType.ExtractBlameForEachPeriod)
            {
                var cmd = new GetGitBlobsAndTheirBlamesForPeriodsCommand(logger);
                await cmd.Execute(options.RepositoryPath, options.GitBranch, options.Extensions.ToArray());
            }
            else if (options.Command.ToLower() == CommandType.Periodize)
            {
                var cmd = new PeriodizeGitCommits();
                await cmd.Execute(options.RepositoryPath, options.GitBranch,options.PeriodType,options.PeriodLength);
            }
            else if (options.Command.ToLower() == CommandType.DoNameAliasing)
            {
                var cmd = new AliasGitNamesCommand();
                await cmd.Execute();
            }
            else if (options.Command.ToLower() == CommandType.ApplyNameAliasing)
            {
                var cmd = new ApplyNameAliasingCommand();
                await cmd.Execute();
            }
            else if (options.Command.ToLower() == CommandType.ExtractDeveloperInformation)
            {
                var cmd = new ExtractDeveloperInformationCommand();
                await cmd.Execute(options.CoreDeveloperThreshold,options.CoreDeveloperCalculationType);
            }
            else if (options.Command.ToLower() == CommandType.IgnoreMegaCommitsAndDevelopers)
            {
                var cmd = new IgnoreMegaCommitsCommand();
                await cmd.Execute(options.MegaCommitSize,options.MegaDevelopers);
            }  
            else if (options.Command.ToLower() == CommandType.MapGitHubGitNames)
            {
                var cmd = new MapGitHubGitNamesCommand();
                await cmd.Execute(options.GitHubToken, agenName:"mirsaeedi", options.GitHubOwner, options.GitHubRepo);
            }
            else if (options.Command.ToLower() == CommandType.ComputeKnowledgeLoss)
            {
                var cmd = new ShareKnowledgeCommand();

                var lossSimulationOption = new LossSimulationOption()
                {
                    KnowledgeShareStrategyType=options.KnowledgeSaveStrategyType,
                    MegaPullRequestSize=options.MegaPullRequestSize,
                    LeaversType=options.LeaversType,
                    FilesAtRiksOwnershipThreshold = options.FilesAtRiksOwnershipThreshold,
                    FilesAtRiksOwnersThreshold = options.FilesAtRiksOwnersThreshold,
                    LeaversOfPeriodExtendedAbsence = options.LeaversOfPeriodExtendedAbsence
                };

                await cmd.Execute(lossSimulationOption);
            }
        }
    }
}
