using Microsoft.Extensions.Logging;
using RelationalGit.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RelationalGit.Recommendation
{
    public class SofiaRecommendationStrategy : ScoreBasedRecommendationStrategy
    {
        private int? _numberOfPeriodsForCalculatingProbabilityOfStay;
        private double _alpha;
        private double _beta;
        private int _riskOwenershipThreshold;
        private double _hoarderRatio;

        public SofiaRecommendationStrategy(string knowledgeSaveReviewerReplacementType, 
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
            _riskOwenershipThreshold = parameters.RiskOwenershipThreshold;
            _hoarderRatio = parameters.HoarderRatio;
        }

        private (double Alpha,double Beta,int RiskOwenershipThreshold,double HoarderRatio) GetParameters(string recommenderOption)
        {
            if (string.IsNullOrEmpty(recommenderOption))
                return (0.5, 1,3,0.7);

            var options = recommenderOption.Split(',');
            var alphaOption = options.FirstOrDefault(q => q.StartsWith("alpha")).Substring("alpha".Length+1);
            var betaOption = options.FirstOrDefault(q => q.StartsWith("beta")).Substring("beta".Length + 1);
            var riskOwenershipThreshold = options.FirstOrDefault(q => q.StartsWith("risk")).Substring("risk".Length + 1);
            var hoarderRatioOption = options.FirstOrDefault(q => q.StartsWith("hoarder_ratio")).Substring("hoarder_ratio".Length + 1);

            return (double.Parse(alphaOption), double.Parse(betaOption),int.Parse(riskOwenershipThreshold),double.Parse(hoarderRatioOption));
        }

        internal override double ComputeReviewerScore(PullRequestContext pullRequestContext, DeveloperKnowledge reviewer)
        {
            double spreadingScore = GetPersistSpreadingScore(pullRequestContext, reviewer);

            var expertiseScore = ComputeBirdReviewerScore(pullRequestContext, reviewer);

            var alpha = pullRequestContext.GetRiskyFiles(_riskOwenershipThreshold).Length > 0 ? 1 : 0;

            var score = (1 - alpha) * expertiseScore + alpha * spreadingScore;

            return score;
        }

        private double GetPersistSpreadingScore(PullRequestContext pullRequestContext, DeveloperKnowledge reviewer)
        {
            var reviewerImportance = pullRequestContext.IsHoarder(reviewer.DeveloperName) ? _hoarderRatio : 1;

            var probabilityOfStay = pullRequestContext.GetProbabilityOfStay(reviewer.DeveloperName, _numberOfPeriodsForCalculatingProbabilityOfStay.Value);
            var effort = pullRequestContext.GetEffort(reviewer.DeveloperName, _numberOfPeriodsForCalculatingProbabilityOfStay.Value);

            var prFiles = pullRequestContext.PullRequestFiles.Select(q => pullRequestContext.CanononicalPathMapper[q.FileName])
                    .Where(q => q != null).ToArray();
            var reviewedFiles = reviewer.GetTouchedFiles().Where(q => prFiles.Contains(q));
            var specializedKnowledge = reviewedFiles.Count() / (double)pullRequestContext.PullRequestFiles.Length;

            var spreadingScore = 0.0;

            spreadingScore = reviewerImportance * Math.Pow(probabilityOfStay * effort, _alpha) * Math.Pow(1 - specializedKnowledge, _beta);

            return spreadingScore;
        }

        private double ComputeBirdReviewerScore(PullRequestContext pullRequestContext, DeveloperKnowledge reviewer)
        {
            var score = 0.0;

            foreach (var pullRequestFile in pullRequestContext.PullRequestFiles)
            {
                var canonicalPath = pullRequestContext.CanononicalPathMapper.GetValueOrDefault(pullRequestFile.FileName);
                if (canonicalPath == null)
                {
                    continue;
                }

                var fileExpertise = pullRequestContext.KnowledgeMap.PullRequestEffortKnowledgeMap.GetFileExpertise(canonicalPath);

                if (fileExpertise.TotalComments == 0)
                {
                    continue;
                }

                var reviewerExpertise = pullRequestContext.KnowledgeMap.PullRequestEffortKnowledgeMap.GetReviewerExpertise(canonicalPath, reviewer.DeveloperName);

                if (reviewerExpertise == (0, 0, null))
                {
                    continue;
                }

                var scoreTotalComments = reviewerExpertise.TotalComments / (double)fileExpertise.TotalComments;
                var scoreTotalWorkDays = reviewerExpertise.TotalWorkDays / (double)fileExpertise.TotalWorkDays;
                var scoreRecency = (fileExpertise.RecentWorkDay == reviewerExpertise.RecentWorkDay)
                    ? 1
                    : 1 / (fileExpertise.RecentWorkDay - reviewerExpertise.RecentWorkDay).Value.TotalDays;

                score += scoreTotalComments + scoreTotalWorkDays + scoreRecency;
            }

            return score / (3 * pullRequestContext.PullRequestFiles.Length);
        }
    }
}
