using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RelationalGit.KnowledgeShareStrategies.Strategies.Spreading
{
    public class PersistSpreadingBasedSpreadingKnowledgeShareStrategy : ScoreBasedSpreadingKnowledgeShareStrategy
    {
        private int? _numberOfPeriodsForCalculatingProbabilityOfStay;

        public PersistSpreadingBasedSpreadingKnowledgeShareStrategy(string knowledgeSaveReviewerReplacementType, ILogger logger, int? numberOfPeriodsForCalculatingProbabilityOfStay, string pullRequestReviewerSelectionStrategy,bool? addOnlyToUnsafePullrequests)
            : base(knowledgeSaveReviewerReplacementType, logger,pullRequestReviewerSelectionStrategy,addOnlyToUnsafePullrequests)
        {
            _numberOfPeriodsForCalculatingProbabilityOfStay = numberOfPeriodsForCalculatingProbabilityOfStay;
        }

        internal override double ComputeReviewerScore(PullRequestContext pullRequestContext, DeveloperKnowledge reviewer)
        {
            var reviewerImportance = pullRequestContext.IsHoarder(reviewer.DeveloperName) ? 0.7 : 1;
            var probabilityOfStay = pullRequestContext.GetProbabilityOfStay(reviewer.DeveloperName, _numberOfPeriodsForCalculatingProbabilityOfStay.Value);
            var effort = pullRequestContext.GetEffort(reviewer.DeveloperName, _numberOfPeriodsForCalculatingProbabilityOfStay.Value);
            var specializedKnowledge = reviewer.NumberOfTouchedFiles / (double)pullRequestContext.PullRequestFiles.Length;

            var score = 0.0;

            if (specializedKnowledge > 1) // if it's a folder level dev
            {
                score = reviewerImportance * Math.Pow(probabilityOfStay * effort, 0.5);
            }
            else
            {
                score = reviewerImportance * Math.Pow(probabilityOfStay * effort, 0.5) * (1 - specializedKnowledge);
            }

            return score;
        }
    }
}
