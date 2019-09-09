using CommandLine;
using System.Collections.Generic;
using System.Linq;

namespace RelationalGit.CommandLine
{
    internal class InputOption
    {
        
        [Option("conf-path")]
        public string AppsettingsPath { get; set; }

        [Option("cmd")]
        public string Command { get; set; }

        [Option("repo-path")]
        public string RepositoryPath { get; set; }

        [Option("github-token")]
        public string GitHubToken { get; set; }

        [Option("git-branch")]
        public string GitBranch { get; set; }

        [Option("github-owner")]
        public string GitHubOwner { get; set; }

        [Option("github-repo")]
        public string GitHubRepo { get; set; }

        [Option("sha")]
        public string CommitSha { get; set; }

        [Option("extensions")]
        public IEnumerable<string> Extensions { get; set; }

        [Option("issue-states")]
        public IEnumerable<string> IssueLabels { get; set; }

        public string IssueState { get; set; }

        [Option("period-type")]
        public string PeriodType { get; set; }

        [Option("period-length")]
        public int? PeriodLength { get; set; }

        [Option("core-dev-threshold")]
        public double? CoreDeveloperThreshold { get; set; }

        [Option("mega-commit-size")]
        public int? MegaCommitSize { get; set; }

        [Option("save-strategy")]
        public string KnowledgeSaveStrategyType { get; set; }

        [Option("replacement-strategy")]
        public string KnowledgeSaveReviewerReplacementType { get; set; }

        [Option("mega-pr-size")]
        public int? MegaPullRequestSize { get; set; }

        [Option("leavers-type")]
        public string LeaversType { get; set; }

        [Option("core-dev-calculatuon-type")]
        public string CoreDeveloperCalculationType { get; set; }

        [Option("file-risk-ownership-threshold")]
        public double? FilesAtRiksOwnershipThreshold { get; set; }

        [Option("file-risk-owners-threshold")]
        public int? FilesAtRiksOwnersThreshold { get; set; }

        [Option("leavers-extention")]
        public int? LeaversOfPeriodExtendedAbsence { get; set; }

        [Option("mega-devs")]
        public IEnumerable<string> MegaDevelopers { get; set; }

        [Option("lgtm-terms")]
        public IEnumerable<string> LgtmTerms { get; set; }

        [Option("blame-periods")]
        public IEnumerable<int> BlamePeriods { get; set; }

        [Option("blame-periods-range")]
        public IEnumerable<int> BlamePeriodsRange{ get; set; }

        [Option("exclude-blame-path")]
        public IEnumerable<string> ExcludeBlamePath { get; set; }

        [Option("simulation-first-period")]
        public int? KnowledgeSaveReviewerFirstPeriod { get; set; }

        [Option("minimum-actual-reviewers")]
        public int? MinimumActualReviewersLength { get; set; }

        [Option("pullRequests-reviewer-selection")]
        public string PullRequestReviewerSelectionStrategy { get; set; }

        [Option("periods-stay-probability")]
        public int? NumberOfPeriodsForCalculatingProbabilityOfStay { get; set; }

        [Option("selected-reviewers-type")]
        public string SelectedReviewersType { get; set; }

        [Option("extract-blames")]
        public bool? ExtractBlames { get; set; }

        [Option("recommender-option")]
        public string RecommenderOption { get;  set; }

        [Option("add-only-to-unsafe-pullrequests")]
        public bool? AddOnlyToUnsafePullrequests { get; set; }

        [Option("change-past")]
        public bool? ChangePast { get; set; }

        [Option("actual-simulation")]
        public long ActualSimulationId { get; set; }

        [Option("recommender-simulation")]
        public IEnumerable<long> RecommenderSimulationId { get; set; }

        [Option("analyze-result-path")]
        public string AnalyzeResultPath { get; set; }

        internal InputOption Override(InputOption fileConfigurationOption)
        {
            var overridedInputOption = new InputOption()
            {
                Command = Command,
                AppsettingsPath = AppsettingsPath
            };

            overridedInputOption.CoreDeveloperCalculationType = Override(CoreDeveloperCalculationType, fileConfigurationOption.CoreDeveloperCalculationType);
            overridedInputOption.CommitSha = Override(CommitSha, fileConfigurationOption.CommitSha);
            overridedInputOption.CoreDeveloperThreshold = Override(CoreDeveloperThreshold, fileConfigurationOption.CoreDeveloperThreshold);
            overridedInputOption.Extensions = Override(Extensions, fileConfigurationOption.Extensions);
            overridedInputOption.FilesAtRiksOwnershipThreshold = Override(FilesAtRiksOwnershipThreshold, fileConfigurationOption.FilesAtRiksOwnershipThreshold);
            overridedInputOption.FilesAtRiksOwnersThreshold = Override(FilesAtRiksOwnersThreshold, fileConfigurationOption.FilesAtRiksOwnersThreshold);
            overridedInputOption.GitBranch = Override(GitBranch, fileConfigurationOption.GitBranch);
            overridedInputOption.GitHubOwner = Override(GitHubOwner, fileConfigurationOption.GitHubOwner);
            overridedInputOption.GitHubRepo = Override(GitHubRepo, fileConfigurationOption.GitHubRepo);
            overridedInputOption.GitHubToken = Override(GitHubToken, fileConfigurationOption.GitHubToken);
            overridedInputOption.KnowledgeSaveStrategyType = Override(KnowledgeSaveStrategyType, fileConfigurationOption.KnowledgeSaveStrategyType);
            overridedInputOption.KnowledgeSaveReviewerReplacementType = Override(KnowledgeSaveReviewerReplacementType, fileConfigurationOption.KnowledgeSaveReviewerReplacementType);
            overridedInputOption.LeaversOfPeriodExtendedAbsence = Override(LeaversOfPeriodExtendedAbsence, fileConfigurationOption.LeaversOfPeriodExtendedAbsence);
            overridedInputOption.LeaversType = Override(LeaversType, fileConfigurationOption.LeaversType);
            overridedInputOption.MegaCommitSize = Override(MegaCommitSize, fileConfigurationOption.MegaCommitSize);
            overridedInputOption.MegaDevelopers = Override(MegaDevelopers, fileConfigurationOption.MegaDevelopers);
            overridedInputOption.MegaPullRequestSize = Override(MegaPullRequestSize, fileConfigurationOption.MegaPullRequestSize);
            overridedInputOption.PeriodLength = Override(PeriodLength, fileConfigurationOption.PeriodLength);
            overridedInputOption.PeriodType = Override(PeriodType, fileConfigurationOption.PeriodType);
            overridedInputOption.RepositoryPath = Override(RepositoryPath, fileConfigurationOption.RepositoryPath);
            overridedInputOption.IssueLabels = Override(IssueLabels, fileConfigurationOption.IssueLabels);
            overridedInputOption.IssueState = Override(IssueState, fileConfigurationOption.IssueState);
            overridedInputOption.ExcludeBlamePath = Override(ExcludeBlamePath, fileConfigurationOption.ExcludeBlamePath);
            overridedInputOption.BlamePeriodsRange = Override(BlamePeriodsRange, fileConfigurationOption.BlamePeriodsRange);
            overridedInputOption.BlamePeriods = Override(BlamePeriods, fileConfigurationOption.BlamePeriods);
            overridedInputOption.KnowledgeSaveReviewerFirstPeriod = Override(KnowledgeSaveReviewerFirstPeriod, fileConfigurationOption.KnowledgeSaveReviewerFirstPeriod);
            overridedInputOption.SelectedReviewersType = Override(SelectedReviewersType, fileConfigurationOption.SelectedReviewersType);
            overridedInputOption.ExtractBlames = Override(ExtractBlames, fileConfigurationOption.ExtractBlames);
            overridedInputOption.LgtmTerms = Override(LgtmTerms, fileConfigurationOption.LgtmTerms);
            overridedInputOption.PullRequestReviewerSelectionStrategy = Override(PullRequestReviewerSelectionStrategy, fileConfigurationOption.PullRequestReviewerSelectionStrategy);
            overridedInputOption.NumberOfPeriodsForCalculatingProbabilityOfStay = Override(NumberOfPeriodsForCalculatingProbabilityOfStay, fileConfigurationOption.NumberOfPeriodsForCalculatingProbabilityOfStay);
            overridedInputOption.MinimumActualReviewersLength = Override(MinimumActualReviewersLength, fileConfigurationOption.MinimumActualReviewersLength);
            overridedInputOption.AddOnlyToUnsafePullrequests = Override(AddOnlyToUnsafePullrequests, fileConfigurationOption.AddOnlyToUnsafePullrequests);
            overridedInputOption.RecommenderOption = Override(RecommenderOption, fileConfigurationOption.RecommenderOption);
            overridedInputOption.ChangePast = Override(ChangePast, fileConfigurationOption.ChangePast);
            overridedInputOption.AnalyzeResultPath = Override(AnalyzeResultPath, fileConfigurationOption.AnalyzeResultPath);
            overridedInputOption.ActualSimulationId = Override(ActualSimulationId, fileConfigurationOption.ActualSimulationId);
            overridedInputOption.RecommenderSimulationId = Override(RecommenderSimulationId, fileConfigurationOption.RecommenderSimulationId);

            return overridedInputOption;
        }

        private T Override<T>(T original, T replace)
        {
            return original != null ? original : replace;
        }

        private IEnumerable<T> Override<T>(IEnumerable<T> original, IEnumerable<T> replace)
        {
            return original?.Count() > 0 ? original : replace;
        }
    }
}
