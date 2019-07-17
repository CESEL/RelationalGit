using Microsoft.Extensions.Logging;
using RelationalGit.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RelationalGit.Recommendation
{
    public class PersistSpreadingBasedSpreadingKnowledgeShareStrategy : ScoreBasedSpreadingKnowledgeShareStrategy
    {
        private int? _numberOfPeriodsForCalculatingProbabilityOfStay;
        private double _alpha;
        private double _beta;

        public PersistSpreadingBasedSpreadingKnowledgeShareStrategy(string knowledgeSaveReviewerReplacementType, 
            ILogger logger, int? numberOfPeriodsForCalculatingProbabilityOfStay, 
            string pullRequestReviewerSelectionStrategy,
            bool? addOnlyToUnsafePullrequests,
            string recommenderOption, bool changePast)
            : base(knowledgeSaveReviewerReplacementType, logger,pullRequestReviewerSelectionStrategy,addOnlyToUnsafePullrequests, recommenderOption,changePast)
        {
            _numberOfPeriodsForCalculatingProbabilityOfStay = numberOfPeriodsForCalculatingProbabilityOfStay;

            var parameters = GetParameters(recommenderOption);
            _alpha = parameters.Alpha;
            _beta = parameters.Beta;
        }

        private (double Alpha,double Beta) GetParameters(string recommenderOption)
        {
            if (string.IsNullOrEmpty(recommenderOption))
                return (0.5, 1);

            var options = recommenderOption.Split(',');
            var alphaOption = options.FirstOrDefault(q => q.StartsWith("alpha")).Substring("alpha".Length+1);
            var betaOption = options.FirstOrDefault(q => q.StartsWith("beta")).Substring("beta".Length + 1);

            return (double.Parse(alphaOption), double.Parse(betaOption));
        }

        internal override double ComputeReviewerScore(PullRequestContext pullRequestContext, DeveloperKnowledge reviewer)
        {
            var reviewerImportance = pullRequestContext.IsHoarder(reviewer.DeveloperName) ? 0.7 : 1;
            reviewerImportance = 1;
            var probabilityOfStay = pullRequestContext.GetProbabilityOfStay(reviewer.DeveloperName, _numberOfPeriodsForCalculatingProbabilityOfStay.Value);
            var effort = pullRequestContext.GetEffort(reviewer.DeveloperName, _numberOfPeriodsForCalculatingProbabilityOfStay.Value);

            var prFiles = pullRequestContext.PullRequestFiles.Select(q => pullRequestContext.CanononicalPathMapper[q.FileName])
                    .Where(q => q != null).ToArray();
            var reviewedFiles = reviewer.GetTouchedFiles().Where(q => prFiles.Contains(q));
            var specializedKnowledge = reviewedFiles.Count() / (double)pullRequestContext.PullRequestFiles.Length;

            var score = 0.0;

            if (specializedKnowledge > 1) // if it's a folder level dev
            {
                score = reviewerImportance * Math.Pow(probabilityOfStay * effort, _alpha);
            }
            else
            {
                score = reviewerImportance * Math.Pow(probabilityOfStay * effort, _alpha) * Math.Pow(1 - specializedKnowledge, _beta);
            }

            return score;
        }
    }
}
