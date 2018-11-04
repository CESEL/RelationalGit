using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RelationalGit
{
    public class RecommendedPullRequestReviewer
    {
        public RecommendedPullRequestReviewer()
        {

        }

        public RecommendedPullRequestReviewer(long pullRequestNumber, string normalizedReviewerName,long lossSimulationId, RecommendedPullRequestReviewerType type)
        {
            PullRequestNumber = pullRequestNumber;
            NormalizedReviewerName = normalizedReviewerName;
            LossSimulationId = lossSimulationId;
            Type = type;
        }

        public long Id { get; set; }

        public long PullRequestNumber { get; internal set; }

        public string NormalizedReviewerName { get; internal set; }

        public long LossSimulationId { get; internal set; }

        public RecommendedPullRequestReviewerType Type { get; set; }
    }

    public enum RecommendedPullRequestReviewerType
    {
        Actual,
        Recommended
    }

}
