using Microsoft.Extensions.Logging;
using RelationalGit.CommandLine;
using System.Linq;
using System.Threading.Tasks;

namespace RelationalGit.Commands
{
    internal class CommandFactory
    {
        public async Task Execute(InputOption options, ILogger logger)
        {
            await RunCommand(options, logger).ConfigureAwait(false);
        }

        private static async Task RunCommand(InputOption options, ILogger logger)
        {
            if (options.Command.ToLower() == CommandType.GetPullRequests)
            {
                var cmd = new GetPullRequestsCommand(logger);
                await cmd.Execute(options.GitHubToken, agenName:"mirsaeedi", options.GitHubOwner, options.GitHubRepo, options.GitBranch).ConfigureAwait(false);
            }
            else if (options.Command.ToLower() == CommandType.GetBlamesOfCommitedChanges)
            {
                var cmd = new GetBlamesOfCommitedChangesCommand(logger);
                await cmd.Execute(options.RepositoryPath, options.GitBranch, options.Extensions.ToArray(), options.ExcludeBlamePath?.ToArray() ?? System.Array.Empty<string>()).ConfigureAwait(false);
            }
            else if (options.Command.ToLower() == CommandType.GetPullRequestReviewes)
            {
                var cmd = new GetPullRequestReviewersCommand(logger);
                await cmd.Execute(options.GitHubToken, agenName: "mirsaeedi", options.GitHubOwner, options.GitHubRepo, options.GitBranch).ConfigureAwait(false);
            }
            else if (options.Command.ToLower() == CommandType.GetPullRequestRevieweComments)
            {
                var cmd = new GetPullRequestReviewerCommentsCommand(logger);
                await cmd.Execute(options.GitHubToken, agenName: "mirsaeedi", options.GitHubOwner, options.GitHubRepo, options.GitBranch).ConfigureAwait(false);
            }
            else if (options.Command.ToLower() == CommandType.GetPullRequestIssueComments)
            {
                var cmd = new GetPullRequestIssueCommentsCommand(logger);
                await cmd.Execute(options.GitHubToken, agenName: "mirsaeedi", options.GitHubOwner, options.GitHubRepo, options.GitBranch).ConfigureAwait(false);
            }
            else if (options.Command.ToLower() == CommandType.GetPullRequestMergeEvents)
            {
                var cmd = new GetPullRequestMergeEventsCommand(logger);
                await cmd.Execute(options.GitHubToken, agenName: "mirsaeedi", options.GitHubOwner, options.GitHubRepo, options.GitBranch).ConfigureAwait(false);
            }
            else if (options.Command.ToLower() == "-get-users")
            {
                var cmd = new GetUsersCommand(logger);
                await cmd.Execute(options.GitHubToken, agentName: "mirsaeedi").ConfigureAwait(false);
            }
            else if (options.Command.ToLower() == CommandType.GetPullRequestFiles)
            {
                var cmd = new GetPullRequestFilesCommand(logger);
                await cmd.Execute(options.GitHubToken, agenName: "mirsaeedi", options.GitHubOwner, options.GitHubRepo, options.GitBranch).ConfigureAwait(false);
            }
            else if (options.Command.ToLower() == "-get-issues")
            {
                var cmd = new GetIssuesCommand(logger);

                await cmd.Execute(options.GitHubToken, agenName: "mirsaeedi", owner: options.GitHubOwner, repo: options.GitHubRepo,
                    labels:options.IssueLabels.ToArray(),
                    state: options.IssueState).ConfigureAwait(false);
            }
            else if (options.Command.ToLower() == "-get-issuesevents")
            {
                var cmd = new GetIssuesEventsCommand(logger);

                await cmd.Execute(options.GitHubToken, agenName: "mirsaeedi", owner: options.GitHubOwner, repo: options.GitHubRepo).ConfigureAwait(false);
            }
            else if (options.Command.ToLower() == CommandType.GetGitCommits)
            {
                var cmd = new GetGitCommitsCommand(logger);
                await cmd.Execute(options.RepositoryPath, options.GitBranch).ConfigureAwait(false);
            }
            else if (options.Command.ToLower() == CommandType.GetGitCommitsChanges)
            {
                var cmd = new GetGitCommitsChangesCommand(logger);
                await cmd.Execute(options.RepositoryPath, options.GitBranch).ConfigureAwait(false);
            }
            else if (options.Command.ToLower() == CommandType.ExtractBlameFromCommit)
            {
                var cmd = new GetGitBlobsAndTheirBlamesOfCommitCommand(logger);
                await cmd.Execute(options.RepositoryPath, options.GitBranch, options.CommitSha, options.Extensions.ToArray(), options.ExcludeBlamePath?.ToArray() ?? System.Array.Empty<string>()).ConfigureAwait(false);
            }
            else if (options.Command.ToLower() == CommandType.ExtractBlameForEachPeriod)
            {
                var cmd = new GetGitBlobsAndTheirBlamesForPeriodsCommand(logger);

                var periods = System.Array.Empty<int>();

                if (options.BlamePeriods != null)
                {
                    periods = options.BlamePeriods?.ToArray();
                }

                if (options.BlamePeriodsRange?.Count() > 0)
                {
                    var count = options.BlamePeriodsRange.ElementAt(1) - options.BlamePeriodsRange.ElementAt(0) + 1;
                    var range = Enumerable.Range(options.BlamePeriodsRange.ElementAt(0), count);

                    periods = periods != null
                        ? periods.Concat(range).ToArray()
                        : range.ToArray();
                }

                var extractBlameForEachPeriodOption = new ExtractBlameForEachPeriodOption()
                {
                    RepositoryPath = options.RepositoryPath,
                    Extensions = options.Extensions.ToArray(),
                    GitBranch = options.GitBranch,
                    PeriodIds = periods,
                    ExcludedBlamePaths = options.ExcludeBlamePath?.ToArray() ?? System.Array.Empty<string>(),
                    ExtractBlames = options.ExtractBlames.Value
                };

                await cmd.Execute(extractBlameForEachPeriodOption).ConfigureAwait(false);
            }
            else if (options.Command.ToLower() == CommandType.Periodize)
            {
                var cmd = new PeriodizeGitCommits(logger);
                await cmd.Execute(options.PeriodType, options.PeriodLength.Value).ConfigureAwait(false);
            }
            else if (options.Command.ToLower() == CommandType.DoNameAliasing)
            {
                var cmd = new AliasGitNamesCommand(logger);
                await cmd.Execute().ConfigureAwait(false);
            }
            else if (options.Command.ToLower() == CommandType.ApplyNameAliasing)
            {
                var cmd = new ApplyNameAliasingCommand(logger);
                await cmd.Execute().ConfigureAwait(false);
            }
            else if (options.Command.ToLower() == CommandType.ExtractDeveloperInformation)
            {
                var cmd = new ExtractDeveloperInformationCommand(logger);
                await cmd.Execute(options.CoreDeveloperThreshold.Value, options.CoreDeveloperCalculationType).ConfigureAwait(false);
            }
            else if (options.Command.ToLower() == CommandType.IgnoreMegaCommitsAndDevelopers)
            {
                var cmd = new IgnoreMegaCommitsCommand(logger);
                await cmd.Execute(options.MegaCommitSize.Value, options.MegaDevelopers).ConfigureAwait(false);
            }
            else if (options.Command.ToLower() == CommandType.MapGitHubGitNames)
            {
                var cmd = new MapGitHubGitNamesCommand(logger);
                await cmd.Execute(options.GitHubToken, agenName:"mirsaeedi", options.GitHubOwner, options.GitHubRepo).ConfigureAwait(false);
            }
            else if (options.Command.ToLower() == CommandType.AnalyzeSimulations)
            {
                var cmd = new AnalyzeSimulationsCommand(logger);
                await cmd.Execute(options.ActualSimulationId,options.RecommenderSimulationId.ToArray(),options.AnalyzeResultPath).ConfigureAwait(false);
            }
            else if (options.Command.ToLower() == CommandType.ComputeKnowledgeLoss)
            {
                var cmd = new ShareKnowledgeCommand(logger);

                var lossSimulationOption = new LossSimulationOption()
                {
                    KnowledgeShareStrategyType = options.KnowledgeSaveStrategyType,
                    KnowledgeSaveReviewerReplacementType = options.KnowledgeSaveReviewerReplacementType,
                    MegaPullRequestSize = options.MegaPullRequestSize.Value,
                    LeaversType = options.LeaversType,
                    FilesAtRiksOwnershipThreshold = options.FilesAtRiksOwnershipThreshold.Value,
                    FilesAtRiksOwnersThreshold = options.FilesAtRiksOwnersThreshold.Value,
                    LeaversOfPeriodExtendedAbsence = options.LeaversOfPeriodExtendedAbsence.Value,
                    KnowledgeSaveReviewerFirstPeriod = options.KnowledgeSaveReviewerFirstPeriod.Value,
                    SelectedReviewersType = options.SelectedReviewersType,
                    PullRequestReviewerSelectionStrategy = options.PullRequestReviewerSelectionStrategy,
                    AddOnlyToUnsafePullrequests = options.AddOnlyToUnsafePullrequests,
                    LgtmTerms =options.LgtmTerms.ToArray(),
                    MinimumActualReviewersLength = options.MinimumActualReviewersLength,
                    NumberOfPeriodsForCalculatingProbabilityOfStay = options.NumberOfPeriodsForCalculatingProbabilityOfStay,
                    MegaDevelopers = options.MegaDevelopers,
                    RecommenderOption = options.RecommenderOption,
                    ChangePast = options.ChangePast
                };

                await cmd.Execute(lossSimulationOption).ConfigureAwait(false);
            }
        }
    }
}
