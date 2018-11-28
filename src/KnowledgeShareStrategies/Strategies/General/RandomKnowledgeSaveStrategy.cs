
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using AutoMapper;
using Microsoft.Extensions.Logging;
using RelationalGit.KnowledgeShareStrategies.Models;

namespace RelationalGit
{
    public class RandomKnowledgeShareStrategy : KnowledgeShareStrategy
    {
        private Random random=new Random();

        public RandomKnowledgeShareStrategy(string knowledgeSaveReviewerReplacementType) : base(knowledgeSaveReviewerReplacementType)
        { }

        protected override PullRequestRecommendationResult RecommendReviewers(PullRequestContext pullRequestContext)
        {
            if(pullRequestContext.ActualReviewers.Count()==0)
                return new PullRequestRecommendationResult(new string[0]);

            var availableDevs = pullRequestContext.AvailableDevelopers.Where(q => q.TotalCommits > 10 || q.TotalReviews > 10).ToArray();
            var reviewers = pullRequestContext.ActualReviewers.Select(q => q.DeveloperName).ToArray();

            if (ReviewerReplacementStrategyType == RelationalGit.ReviewerReplacementStrategyType.OneOfActuals)
            {
                return new PullRequestRecommendationResult(OneOfActualsRecomendation(availableDevs, reviewers));
            }
            else if (ReviewerReplacementStrategyType == RelationalGit.ReviewerReplacementStrategyType.AllOfActuals)
            {
                return new PullRequestRecommendationResult(AllOfActualsRecomendation(availableDevs, reviewers));
            }

            throw new Exception($"the specified ReviewerReplacementStrategyType is unknown: {ReviewerReplacementStrategyType}");
        }

        private string[] AllOfActualsRecomendation(Developer[] availableDevs, string[] reviewers)
        {
            var countOfPossibleRecommendation = Math.Min(reviewers.Length, availableDevs.Length);
            var recommendations = new string[countOfPossibleRecommendation];

            for (int i = 0; i < countOfPossibleRecommendation;)
            {
                var selectedReviewer = random.Next(0, reviewers.Length);
                if (!recommendations.Any(q => q == reviewers[selectedReviewer]))
                {
                    recommendations[i] = reviewers[selectedReviewer];
                    i++;
                }
            }

            return recommendations;
        }

        private string[] OneOfActualsRecomendation(Developer[] availableDevs, string[] reviewers)
        {
            var recommendations = (string[]) reviewers.Clone();

            var selectedReviewer = random.Next(0, reviewers.Length);
            var selectedDeveloper = random.Next(0, availableDevs.Length);

            recommendations[selectedReviewer] = availableDevs[selectedDeveloper].NormalizedName;

            return recommendations;
        }
    }
}
