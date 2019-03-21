using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RelationalGit.KnowledgeShareStrategies.Strategies.Spreading
{
    public abstract class ScoreBasedSpreadingKnowledgeShareStrategy : SpreadingKnowledgeShareStrategyBase
    {
        private ILogger _logger;
        private bool? _addOnlyToUnsafePullrequests;
        private readonly PullRequestReviewerSelectionStrategy[] _pullRequestReviewerSelectionStrategies;
        private readonly PullRequestReviewerSelectionStrategy _pullRequestReviewerSelectionDefaultStrategy;
        private static Dictionary<string, int[][]> _combinationDic = new Dictionary<string, int[][]>();

        public ScoreBasedSpreadingKnowledgeShareStrategy(string knowledgeSaveReviewerReplacementType, ILogger logger, string pullRequestReviewerSelectionStrategy,bool? addOnlyToUnsafePullrequests)
            : base(knowledgeSaveReviewerReplacementType, logger)
        {
            _logger = logger;

            _pullRequestReviewerSelectionStrategies = ParsePullRequestReviewerSelectionStrategy(pullRequestReviewerSelectionStrategy);
            _pullRequestReviewerSelectionDefaultStrategy = _pullRequestReviewerSelectionStrategies.Single(q => q.ActualReviewerCount == "-");
            _addOnlyToUnsafePullrequests = addOnlyToUnsafePullrequests;
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
                    IsDeveloperAvailable(pullRequestContext, q.DeveloperName) &&
                    IsCoreDeveloper(pullRequestContext, q.DeveloperName))).ToArray();

                ComputeAllReviewerScores(pullRequestContext, availableDevs);
            }

            return availableDevs.OrderByDescending(q => q.Score).ToArray();
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
            //_logger.LogInformation("{datetime} Finding the best set of reviwers for Pull Request {pullrequest} with {ActualReviewersLength} Reviewers and {availableDevsLength} Candidates"
               // , DateTime.Now, pullRequestContext.PullRequest.Number, pullRequestContext.ActualReviewers.Length, availableDevs.Length);

            var strategy = _pullRequestReviewerSelectionStrategies
                .SingleOrDefault(q => q.ActualReviewerCount == pullRequestContext.ActualReviewers.Length.ToString());

            availableDevs = availableDevs.OrderByDescending(q => q.Score).ToArray();

            var actualReviewersLength = pullRequestContext.ActualReviewers.Length;

            if (ShouldAddReviewer(pullRequestContext, strategy))
            {
                var selectedCandidatesLength = int.Parse(strategy?.ActionArgument ?? _pullRequestReviewerSelectionDefaultStrategy.ActionArgument);
                var selectedCandidates = GetTopCandidates(availableDevs, selectedCandidatesLength, pullRequestContext.ActualReviewers);
                var newReviewerSet = pullRequestContext.ActualReviewers.Concat(selectedCandidates).ToArray();

                yield return (newReviewerSet, selectedCandidates);
            }
            else if (ShouldReplaceReviewer(pullRequestContext, strategy))
            {
                yield return (pullRequestContext.ActualReviewers, null);

                var selectedCandidatesLength = int.Parse(strategy?.ActionArgument ?? _pullRequestReviewerSelectionDefaultStrategy.ActionArgument);
                var numberOfReplacements = Math.Min(availableDevs.Length, selectedCandidatesLength);
                numberOfReplacements = Math.Min(actualReviewersLength, numberOfReplacements);

                var actualReviewersCombination = GetCombinations(actualReviewersLength, numberOfReplacements);

                var reviewerSet = new HashSet<string>();

                foreach (var selectedActualCombination in actualReviewersCombination)
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
                        continue;
                    }

                    var newReviewerSet = fixedReviewers.Concat(selectedCandidates);
                    var newReviewerSetKey = newReviewerSet.Select(q => q.DeveloperName).OrderBy(q => q).Aggregate((a, b) => a + "," + b);

                    if (!reviewerSet.Contains(newReviewerSetKey))
                    {
                        reviewerSet.Add(newReviewerSetKey);
                        yield return (newReviewerSet, selectedCandidates);
                    }
                }
            }

            //_logger.LogInformation("{datetime} All the sets have been calculated", DateTime.Now);
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

        private bool ShouldReplaceReviewer(PullRequestContext pullRequestContext, PullRequestReviewerSelectionStrategy strategy)
        {
            return pullRequestContext.ActualReviewers.Length >= 1
                            && ((strategy != null && strategy.Action == "replace")
                            || (strategy == null && _pullRequestReviewerSelectionDefaultStrategy.Action == "replace"));
        }

        private bool ShouldAddReviewer(PullRequestContext pullRequestContext, PullRequestReviewerSelectionStrategy strategy)
        {
            return (!_addOnlyToUnsafePullrequests.HasValue || !_addOnlyToUnsafePullrequests.Value ||
                            (_addOnlyToUnsafePullrequests.Value && !pullRequestContext.PullRequestFilesAreSafe))
                            && ((strategy != null && strategy.Action == "add")
                            || (strategy == null && _pullRequestReviewerSelectionDefaultStrategy.Action == "add"));
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
