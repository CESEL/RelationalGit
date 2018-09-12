
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

namespace RelationalGit
{
    public class RandomKnowledgeShareStrategy : KnowledgeShareStrategy
    {
        private Random random=new Random();
        internal override string[] RecommendReviewers(PullRequestContext pullRequestContext)
        {
            if(pullRequestContext.ActualReviewers.Count()==0)
                return new string[0];

            var selectedReviewer = random.Next(0,pullRequestContext.ActualReviewers.Count());
            var selectedDeveloper = random.Next(0,pullRequestContext.AvailableDevelopers.Count());

            pullRequestContext.ActualReviewers[selectedReviewer]
                = pullRequestContext.AvailableDevelopers[selectedDeveloper].NormalizedName;

            return pullRequestContext.ActualReviewers;
        }
    }
}
