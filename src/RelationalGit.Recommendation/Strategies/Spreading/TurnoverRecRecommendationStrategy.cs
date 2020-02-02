using Microsoft.Extensions.Logging;
using RelationalGit.Simulation;
using System;
using System.Linq;

namespace RelationalGit.Recommendation
{
    public class TurnoverRecRecommendationStrategy : ScoreBasedRecommendationStrategy
    {
        private int? _numberOfPeriodsForCalculatingProbabilityOfStay;
        private double _alpha;
        private double _beta;
        private double _hoarderRatio;

        public TurnoverRecRecommendationStrategy(string knowledgeSaveReviewerReplacementType, 
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
            _hoarderRatio = parameters.hoarderRatio;
        }

        private (double Alpha,double Beta,double hoarderRatio) GetParameters(string recommenderOption)
        {
            if (string.IsNullOrEmpty(recommenderOption))
                return (0.5, 1,0.7);

            var options = recommenderOption.Split(',');
            var alphaOption = options.FirstOrDefault(q => q.StartsWith("alpha")).Substring("alpha".Length+1);
            var betaOption = options.FirstOrDefault(q => q.StartsWith("beta")).Substring("beta".Length + 1);
            var hoarderRatioOption = options.FirstOrDefault(q => q.StartsWith("hoarder_ratio")).Substring("hoarder_ratio".Length + 1);

            return (double.Parse(alphaOption), double.Parse(betaOption),double.Parse(hoarderRatioOption));
        }

        internal override double ComputeReviewerScore(PullRequestContext pullRequestContext, DeveloperKnowledge reviewer)
        {
            var reviewerImportance = pullRequestContext.IsHoarder(reviewer.DeveloperName) ? _hoarderRatio : 1;
            
            var probabilityOfStay = pullRequestContext.GetProbabilityOfStay(reviewer.DeveloperName, _numberOfPeriodsForCalculatingProbabilityOfStay.Value);
            var effort = pullRequestContext.GetEffort(reviewer.DeveloperName, _numberOfPeriodsForCalculatingProbabilityOfStay.Value);

            var prFiles = pullRequestContext.PullRequestFiles.Select(q => pullRequestContext.CanononicalPathMapper[q.FileName]).Where(q => q != null).ToArray();

            var reviewedFiles = reviewer.GetTouchedFiles().Where(q => prFiles.Contains(q));
            var specializedKnowledge = reviewedFiles.Count() / (double)pullRequestContext.PullRequestFiles.Length;

            var score = 0.0;

            score = reviewerImportance * Math.Pow(probabilityOfStay * effort, _alpha) * Math.Pow(1 - specializedKnowledge, _beta);

            return score;
        }
    }
}
