using System;
using System.Collections.Generic;
using System.Linq;

namespace RelationalGit.KnowledgeShareStrategies.Strategies.Spreading
{
    public class FileLevelProbabilityBasedSpreadingKnowledgeShareStrategy : SpreadingKnowledgeShareStrategyBase
    {
        private int? _numberOfPeriodsForCalculatingProbabilityOfStay;
        private bool? _addOnlyToUnsafePullrequests;
        private readonly PullRequestReviewerSelectionStrategy[] _pullRequestReviewerSelectionStrategies;
        private readonly PullRequestReviewerSelectionStrategy _pullRequestReviewerSelectionDefaultStrategy;
        private static Dictionary<string, int[][]> _combinationDic = new Dictionary<string, int[][]>();

        public FileLevelProbabilityBasedSpreadingKnowledgeShareStrategy(string knowledgeSaveReviewerReplacementType, int? numberOfPeriodsForCalculatingProbabilityOfStay, string pullRequestReviewerSelectionStrategy,bool? addOnlyToUnsafePullrequests)
            : base(knowledgeSaveReviewerReplacementType)
        {
            _numberOfPeriodsForCalculatingProbabilityOfStay = numberOfPeriodsForCalculatingProbabilityOfStay;
            _pullRequestReviewerSelectionStrategies = ParsePullRequestReviewerSelectionStrategy(pullRequestReviewerSelectionStrategy);
            _pullRequestReviewerSelectionDefaultStrategy = _pullRequestReviewerSelectionStrategies.Single(q => q.ActualReviewerCount == "-");
            _addOnlyToUnsafePullrequests = addOnlyToUnsafePullrequests;
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

        internal override double ComputeScore(PullRequestContext pullRequestContext, PullRequestKnowledgeDistributionFactors pullRequestKnowledgeDistributionFactors)
        {
            var scores = new List<double>();

            foreach (var reviewer in pullRequestKnowledgeDistributionFactors.Reviewers)
            {
                var isSafe = pullRequestContext.PullRequestFilesAreSafe;
                var reviewerImportance = pullRequestContext.IsHoarder(reviewer) ? 0.7 : 1;
                var probabilityOfStay = pullRequestContext.GetProbabilityOfStay(reviewer, _numberOfPeriodsForCalculatingProbabilityOfStay.Value);
                var effort = pullRequestContext.GetEffort(reviewer,_numberOfPeriodsForCalculatingProbabilityOfStay.Value);
                var specializedKnowledge = pullRequestContext.GetSpecializedKnowledge(reviewer);
                var score = 0.0;

                if (!isSafe)
                {
                    score = reviewerImportance * Math.Pow(probabilityOfStay * effort, 0.5) * (1 - specializedKnowledge);
                }
                else
                {
                    score = reviewerImportance * Math.Pow(probabilityOfStay * effort, 0.7) * (1 - specializedKnowledge);
                }

                scores.Add(score);
            }

            return scores.Aggregate((a, b) => a + b);
        }

        internal override IEnumerable<(string[] Reviewers, DeveloperKnowledge SelectedCandidateKnowledge)> GetPossibleCandidateSets(PullRequestContext pullRequestContext, DeveloperKnowledge[] availableDevs, PullRequestKnowledgeDistribution prereviewKnowledgeDistribution)
        {
            var strategy = _pullRequestReviewerSelectionStrategies
                .SingleOrDefault(q => q.ActualReviewerCount == pullRequestContext.ActualReviewers.Length.ToString());

            if ((!_addOnlyToUnsafePullrequests.HasValue || !_addOnlyToUnsafePullrequests.Value ||
                (_addOnlyToUnsafePullrequests.Value && !pullRequestContext.PullRequestFilesAreSafe))
                && ((strategy != null && strategy.Action == "add")
                || (strategy == null && _pullRequestReviewerSelectionDefaultStrategy.Action == "add")))
            {
                var reviewerSet = new HashSet<string>();
                var count = int.Parse(strategy?.ActionArgument ?? _pullRequestReviewerSelectionDefaultStrategy.ActionArgument);
                var candidateReviewersCombination = GetNextCombination(availableDevs.Length, Math.Min(availableDevs.Length, count));
                foreach (var candidateReviewers in candidateReviewersCombination)
                {
                    var candidates = new List<DeveloperKnowledge>(10);

                    for (int i = 0; i < count; i++)
                    {
                        candidates.Add(availableDevs[candidateReviewers[i]]);
                    }

                    var newReviewerSet = pullRequestContext.ActualReviewers.Concat(candidates).Select(q => q.DeveloperName)
                        .OrderBy(q => q).ToArray();

                    if (newReviewerSet.GroupBy(q => q).Any(g => g.Count() > 1))
                    {
                        continue;
                    }

                    var newReviewerSetKey = newReviewerSet.Aggregate((a, b) => a + "," + b);

                    if (!reviewerSet.Contains(newReviewerSetKey))
                    {
                        reviewerSet.Add(newReviewerSetKey);
                        yield return (newReviewerSet, null);
                    }
                }
            }
            else if (pullRequestContext.ActualReviewers.Length >= 1
                && ((strategy != null && strategy.Action == "replace")
                || (strategy == null && _pullRequestReviewerSelectionDefaultStrategy.Action == "replace")))
            {
                var count = int.Parse(strategy?.ActionArgument ?? _pullRequestReviewerSelectionDefaultStrategy.ActionArgument);
                var numberOfReplacements = Math.Min(availableDevs.Length, count);
                numberOfReplacements = Math.Min(pullRequestContext.ActualReviewers.Length, numberOfReplacements);

                var changeableReviewersBase = (string[])pullRequestContext.ActualReviewers.Select(q => q.DeveloperName).ToArray().Clone();

                yield return ((string[] Reviewers, DeveloperKnowledge SelectedCandidateKnowledge))(changeableReviewersBase.ToArray().Clone(), null);

                var actualReviewersCombination = GetNextCombination(changeableReviewersBase.Length, numberOfReplacements);
                var candidateReviewersCombination = GetNextCombination(availableDevs.Length, numberOfReplacements);

                var reviewerSet = new HashSet<string>();

                foreach (var selectedActualCombination in actualReviewersCombination)
                {
                    var selectedActualReviewers = new string[numberOfReplacements];

                    for (int i = 0; i < numberOfReplacements; i++) // memorizing the actual state
                    {
                        selectedActualReviewers[i] = changeableReviewersBase[selectedActualCombination[i]];
                    }

                    foreach (var selectedCandidateCombination in candidateReviewersCombination)
                    {
                        for (int i = 0; i < numberOfReplacements; i++)
                        {
                            changeableReviewersBase[selectedActualCombination[i]] = availableDevs[selectedCandidateCombination[i]].DeveloperName;
                        }

                        if (changeableReviewersBase.GroupBy(q => q).Any(g => g.Count() > 1))
                        {
                            continue;
                        }

                        var newReviewerSet = ((string[])changeableReviewersBase.ToArray().Clone()).OrderBy(q => q).ToArray();
                        var newReviewerSetKey = newReviewerSet.Aggregate((a,b) => a + "," + b);

                        if (!reviewerSet.Contains(newReviewerSetKey))
                        {
                            reviewerSet.Add(newReviewerSetKey);
                            yield return (newReviewerSet, null);
                        }
                    }

                    for (int i = 0; i < numberOfReplacements; i++) // restoring to the actual state
                    {
                        changeableReviewersBase[selectedActualCombination[i]] = selectedActualReviewers[i];
                    }
                }
            }
        }

        private IEnumerable<int[]> GetNextCombination(int length, int n)
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

        internal override DeveloperKnowledge[] AvailablePRKnowledgeables(PullRequestContext pullRequestContext)
        {
            return pullRequestContext.PullRequestKnowledgeables.Where(q => q.DeveloperName != pullRequestContext.PRSubmitterNormalizedName &&
              IsDeveloperAvailable(pullRequestContext, q.DeveloperName) &&
             // !IsDevelperAmongActualReviewers(pullRequestContext, q.DeveloperName) &&
              IsCoreDeveloper(pullRequestContext, q.DeveloperName)).ToArray();
        }
    }
}
