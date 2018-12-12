using System;
using System.Linq;
using AutoMapper;
using RelationalGit.KnowledgeShareStrategies.Models;

namespace RelationalGit
{
    public class RandomKnowledgeShareStrategy : KnowledgeShareStrategy
    {
        private static Random _random = new Random();

        public RandomKnowledgeShareStrategy(string knowledgeSaveReviewerReplacementType)
            : base(knowledgeSaveReviewerReplacementType)
        {
        }

        protected override PullRequestRecommendationResult RecommendReviewers(PullRequestContext pullRequestContext)
        {
            if(pullRequestContext.ActualReviewers.Count() == 0)
            {
                return new PullRequestRecommendationResult(new string[0]);
            }

            var availableDevs = pullRequestContext.AvailableDevelopers.Where(q => IsCoreDeveloper(pullRequestContext,q.NormalizedName)).ToArray();
            var reviewers = pullRequestContext.ActualReviewers.Select(q => q.DeveloperName).ToArray();

            if (ReviewerReplacementStrategyType == RelationalGit.ReviewerReplacementStrategyType.OneOfActuals)
            {
                return new PullRequestRecommendationResult(OneOfActualsRecomendation(availableDevs, reviewers), availableDevs.Select(q => q.NormalizedName).ToArray());
            }
            else if (ReviewerReplacementStrategyType == RelationalGit.ReviewerReplacementStrategyType.AllOfActuals)
            {
                return new PullRequestRecommendationResult(AllOfActualsRecomendation(availableDevs, reviewers), availableDevs.Select(q => q.NormalizedName).ToArray());
            }
            else if (ReviewerReplacementStrategyType == RelationalGit.ReviewerReplacementStrategyType.AddNewReviewerToActuals)
            {
                return new PullRequestRecommendationResult(AddNewReviewerToActualsRecommendation(availableDevs, reviewers), availableDevs.Select(q => q.NormalizedName).ToArray());
            }

            throw new Exception($"the specified ReviewerReplacementStrategyType is unknown: {ReviewerReplacementStrategyType}");
        }

        private string[] AddNewReviewerToActualsRecommendation(Developer[] availableDevs, string[] reviewers)
        {
            var possibleCandidates = availableDevs.Where(q => reviewers.All(r => r != q.NormalizedName)).ToArray();

            if (possibleCandidates.Length == 0)
            {
                return reviewers;
            }

            var selectedDeveloper = _random.Next(0, possibleCandidates.Length);
            return reviewers.Concat(new[]{ possibleCandidates[selectedDeveloper].NormalizedName}).ToArray();

        }

        private string[] AllOfActualsRecomendation(Developer[] availableDevs, string[] reviewers)
        {
            var countOfPossibleRecommendation = Math.Min(reviewers.Length, availableDevs.Length);
            var recommendations = new string[countOfPossibleRecommendation];

            for (int i = 0; i < countOfPossibleRecommendation;)
            {
                var selectedReviewer = _random.Next(0, availableDevs.Length);
                if (!recommendations.Any(q => q == availableDevs[selectedReviewer].NormalizedName))
                {
                    recommendations[i] = availableDevs[selectedReviewer].NormalizedName;
                    i++;
                }
            }

            return recommendations;
        }

        private string[] OneOfActualsRecomendation(Developer[] availableDevs, string[] reviewers)
        {
            var possibleCandidates = availableDevs.Where(q => reviewers.All(r => r != q.NormalizedName)).ToArray();

            var recommendations = (string[])reviewers.Clone();

            var selectedReviewer = _random.Next(0, reviewers.Length);
            var selectedDeveloper = _random.Next(0, possibleCandidates.Length);

            recommendations[selectedReviewer] = possibleCandidates[selectedDeveloper].NormalizedName;

            return recommendations;
        }
    }
}
