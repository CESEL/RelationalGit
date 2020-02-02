using Microsoft.Extensions.Logging;
using RelationalGit.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RelationalGit.Recommendation
{
    public abstract class ScoreBasedRecommendationStrategy : SpreadingKnowledgeShareStrategyBase
    {
        private static Random _rnd = new Random();
        private ILogger _logger;
        private bool? _addOnlyToUnsafePullrequests;
        private readonly PullRequestReviewerSelectionStrategy[] _pullRequestReviewerSelectionStrategies;
        private readonly PullRequestReviewerSelectionStrategy _pullRequestReviewerSelectionDefaultStrategy;
        private static Dictionary<string, int[][]> _combinationDic = new Dictionary<string, int[][]>();

        protected string RrecommenderOption { get; private set; }

        public ScoreBasedRecommendationStrategy(string knowledgeSaveReviewerReplacementType,
            ILogger logger, 
            string pullRequestReviewerSelectionStrategy,
            bool? addOnlyToUnsafePullrequests,
            string recommenderOption,
            bool changePast)
            : base(knowledgeSaveReviewerReplacementType, logger,changePast)
        {
            _logger = logger;

            _pullRequestReviewerSelectionStrategies = ParsePullRequestReviewerSelectionStrategy(pullRequestReviewerSelectionStrategy);
            _pullRequestReviewerSelectionDefaultStrategy = _pullRequestReviewerSelectionStrategies.Single(q => q.ActualReviewerCount == "-");
            _addOnlyToUnsafePullrequests = addOnlyToUnsafePullrequests;
            RrecommenderOption = recommenderOption;
        }

        private void ComputeAllReviewerScores(PullRequestContext pullRequestContext, DeveloperKnowledge[] availableDevs)
        {
            foreach (var candidate in availableDevs)
            {
                var score = ComputeReviewerScore(pullRequestContext, candidate);
                candidate.Score = score;
            }
        }

        internal override sealed DeveloperKnowledge[] AvailablePRKnowledgeables(PullRequestContext pullRequestContext)
        {
            var availableDevs = pullRequestContext.PullRequestKnowledgeables.Where(q =>
                q.DeveloperName != pullRequestContext.PRSubmitterNormalizedName &&
                IsDeveloperAvailable(pullRequestContext, q.DeveloperName)).ToArray();

            ComputeAllReviewerScores(pullRequestContext, availableDevs);

            var depthToScanForReviewers = 0;

            while (availableDevs.All(q => q.Score == 0) && depthToScanForReviewers < 5)
            {
                depthToScanForReviewers++;
                var folderLevelOwners = GetFolderLevelOweners(depthToScanForReviewers, pullRequestContext)
                    .Where(q => availableDevs.All(ad => q.DeveloperName != ad.DeveloperName));

                availableDevs = availableDevs.Concat(folderLevelOwners
                    .Where(q => q.DeveloperName != pullRequestContext.PRSubmitterNormalizedName &&
                    IsDeveloperAvailable(pullRequestContext, q.DeveloperName))).ToArray();

                ComputeAllReviewerScores(pullRequestContext, availableDevs);
            }

            return availableDevs
                .OrderByDescending(q => q.Score)
                .ToArray();
        }

        internal override sealed double ComputeScore(PullRequestContext pullRequestContext, PullRequestKnowledgeDistributionFactors pullRequestKnowledgeDistributionFactors)
        {
            var scores = new List<double>();

            foreach (var reviewer in pullRequestKnowledgeDistributionFactors.Reviewers)
            {
                double score = reviewer.Score == 0 ? ComputeReviewerScore(pullRequestContext, reviewer) : reviewer.Score;

                scores.Add(score);
            }

            return scores.Aggregate((a, b) => a + b);
        }

        private PullRequestReviewerSelectionStrategy[] ParsePullRequestReviewerSelectionStrategy(string pullRequestReviewerSelectionStrategy)
        {
            var rawStrategies = pullRequestReviewerSelectionStrategy.Split(',');
            var strategies = new List<PullRequestReviewerSelectionStrategy>();

            foreach (var strategy in rawStrategies)
            {
                var parts = strategy.Split(':');
                var actualReviewerCount = parts[0];
                var actionDetail = parts[1].Split('-');
                var action = actionDetail[0];
                var actionArgument = actionDetail[1];

                strategies.Add(new PullRequestReviewerSelectionStrategy
                {
                    ActualReviewerCount = actualReviewerCount,
                    Action = action,
                    ActionArgument = actionArgument
                });
            }

            return strategies.ToArray();
        }

        internal override sealed IEnumerable<(IEnumerable<DeveloperKnowledge> Reviewers, IEnumerable<DeveloperKnowledge> SelectedCandidateKnowledge)> GetPossibleCandidateSets(PullRequestContext pullRequestContext, DeveloperKnowledge[] availableDevs)
        {
            var result = new List<(IEnumerable<DeveloperKnowledge> Reviewers, IEnumerable<DeveloperKnowledge> SelectedCandidateKnowledge)>();
            var strategies = GetModificationStrategies(pullRequestContext);

            availableDevs = availableDevs.OrderByDescending(q => q.Score).ToArray();

            var actualReviewersLength = pullRequestContext.ActualReviewers.Length;

            if (ShouldAddReviewer(pullRequestContext, strategies))
            {
                var selectedCandidatesLength = GetSelectedCandidatesLength(pullRequestContext, strategies,"add");
                var selectedCandidates = GetTopCandidates(availableDevs, selectedCandidatesLength, pullRequestContext.ActualReviewers);
                var newReviewerSet = pullRequestContext.ActualReviewers.Concat(selectedCandidates).ToArray();

                result.Add((newReviewerSet, selectedCandidates));
            }

            if (ShouldReplaceReviewer(pullRequestContext, strategies))
            {
                var selectedCandidatesLength = GetSelectedCandidatesLength(pullRequestContext, strategies, "replace");
                var numberOfReplacements = Math.Min(availableDevs.Length, selectedCandidatesLength);
                numberOfReplacements = Math.Min(actualReviewersLength, numberOfReplacements);

                var actualReviewersCombination = GetCombinations(actualReviewersLength, numberOfReplacements);

                var reviewerSet = new HashSet<string>();

                if (IsRandomReplacement(pullRequestContext, strategies))
                {
                    var selectedActualCombination = actualReviewersCombination.ElementAt(_rnd.Next(0, actualReviewersCombination.Count()));

                    var replacement = Replace(numberOfReplacements, actualReviewersLength, selectedActualCombination, pullRequestContext, availableDevs);

                    if (replacement.SelectedCandidateKnowledge.Count() != 0)
                    {
                        result.Add((replacement.Reviewers, replacement.SelectedCandidateKnowledge));
                    }
                }
                else
                {
                    foreach (var selectedActualCombination in actualReviewersCombination)
                    {
                        var replacement = Replace(numberOfReplacements, actualReviewersLength, selectedActualCombination, pullRequestContext, availableDevs);

                        if (replacement.SelectedCandidateKnowledge.Count() == 0)
                            continue;

                        var newReviewerSet = replacement.Reviewers;
                        var newReviewerSetKey = newReviewerSet.Select(q => q.DeveloperName).OrderBy(q => q).Aggregate((a, b) => a + "," + b);

                        if (!reviewerSet.Contains(newReviewerSetKey))
                        {
                            reviewerSet.Add(newReviewerSetKey);
                            result.Add((newReviewerSet, replacement.SelectedCandidateKnowledge));
                        }
                    }
                }
            }

            return result;
        }

        private PullRequestReviewerSelectionStrategy[] GetModificationStrategies(PullRequestContext pullRequestContext)
        {
            var strategies = _pullRequestReviewerSelectionStrategies
                .Where(q => q.ActualReviewerCount == pullRequestContext.ActualReviewers.Length.ToString())
                .ToArray();

            if (strategies.Length > 2)
                throw new InvalidOperationException($"There are more than two modification strategies for : {pullRequestContext.ActualReviewers.Length}");

            if (strategies.Length == 2)
            {
                if(strategies[0].Action==strategies[1].Action)
                    throw new InvalidOperationException($"There are duplicated modification strategies for : {pullRequestContext.ActualReviewers.Length}");
            }

            return strategies;
        }

        private int GetSelectedCandidatesLength(PullRequestContext pullRequestContext, PullRequestReviewerSelectionStrategy[] strategies,string policy)
        {
            var length = _pullRequestReviewerSelectionDefaultStrategy.ActionArgument;

            if (strategies.Length > 0)
            {
                length = strategies.Single(q => q.Action.StartsWith(policy)).ActionArgument;
            }

            if (length == "all")
                return pullRequestContext.ActualReviewers.Length;

            return int.Parse(length);
        }

        private (IEnumerable<DeveloperKnowledge> Reviewers, IEnumerable<DeveloperKnowledge> SelectedCandidateKnowledge) Replace(int numberOfReplacements, int actualReviewersLength, int[] selectedActualCombination, PullRequestContext pullRequestContext, DeveloperKnowledge[] availableDevs)
        {
            var fixedReviewers = new List<DeveloperKnowledge>(numberOfReplacements);

            for (int i = 0; i < actualReviewersLength; i++)
            {
                if (selectedActualCombination.All(q => q != i))
                {
                    fixedReviewers.Add(pullRequestContext.ActualReviewers[i]);
                }
            }

            var selectedCandidates = GetTopCandidates(availableDevs, numberOfReplacements, fixedReviewers);

            if (selectedCandidates.Count() == 0)
            {
                return (Array.Empty<DeveloperKnowledge>(), Array.Empty<DeveloperKnowledge>());
            }

            var newReviewerSet = fixedReviewers.Concat(selectedCandidates);

            return (newReviewerSet,selectedCandidates);
        }

        private IEnumerable<DeveloperKnowledge> GetTopCandidates(DeveloperKnowledge[] candidates, int count, IEnumerable<DeveloperKnowledge> fixedReviewers)
        {
            var selectedCandidates = new List<DeveloperKnowledge>();
            var index = 0;

            while (selectedCandidates.Count < count && index < candidates.Length)
            {
                var selectedCandidate = candidates[index];

                if (!fixedReviewers.Any(q => q?.DeveloperName == selectedCandidate.DeveloperName))
                {
                    selectedCandidates.Add(selectedCandidate);
                }

                index++;
            }

            return selectedCandidates;
        }

        private bool IsRandomReplacement(PullRequestContext pullRequestContext, PullRequestReviewerSelectionStrategy[] strategies)
        {
            if (pullRequestContext.ActualReviewers.Length == 0)
                return false;

            var result = false;

            if (strategies.Length == 0)
            {
                result = _pullRequestReviewerSelectionDefaultStrategy.Action == "replacerandom";
            }
            else
            {
                result = strategies.Any(q => q.Action == "replacerandom");
            }

            return result;
        }

        private bool ShouldReplaceReviewer(PullRequestContext pullRequestContext, PullRequestReviewerSelectionStrategy[] strategies)
        {

            if (pullRequestContext.ActualReviewers.Length == 0)
                return false;

            var result = false;

            if (strategies.Length == 0)
            {
                result = _pullRequestReviewerSelectionDefaultStrategy.Action.StartsWith("replace");
            }
            else
            {
                result = strategies.Any(q => q.Action.StartsWith("replace"));
            }

            return result;
        }

        private bool ShouldAddReviewer(PullRequestContext pullRequestContext, PullRequestReviewerSelectionStrategy[] strategies)
        {
            var result = false;

            if (strategies.Length == 0)
            {
                result =  _pullRequestReviewerSelectionDefaultStrategy.Action == "add";
            }
            else
            {
                result = strategies.Any(q => q.Action == "add");
            }

            if (result)
            {
                if (!_addOnlyToUnsafePullrequests.HasValue || !_addOnlyToUnsafePullrequests.Value)
                    return true;

                if (_addOnlyToUnsafePullrequests.Value && !pullRequestContext.PullRequestFilesAreSafe)
                    return true;
            }

            return false;
        }

        private IEnumerable<int[]> GetCombinations(int length, int n)
        {
            var combinationKey = length + "-" + n;

            if (!_combinationDic.ContainsKey(combinationKey))
            {
                _combinationDic[combinationKey] = GetNextCombination(length, 0, n, 0, new int[n]).ToArray();
            }

            return _combinationDic[combinationKey];
        }

        private IEnumerable<int[]> GetNextCombination(int length, int i, int n, int currentIndex, int[] selectedIndexes)
        {
            for (; i < length - n + 1; i++)
            {
                selectedIndexes[currentIndex] = i;

                if (n == 1)
                {
                    yield return (int[])selectedIndexes.Clone();
                }
                else
                {
                    foreach (var combination in GetNextCombination(length, i + 1, n - 1, currentIndex + 1, selectedIndexes))
                    {
                        yield return combination;
                    }
                }
            }
        }
    }
}
