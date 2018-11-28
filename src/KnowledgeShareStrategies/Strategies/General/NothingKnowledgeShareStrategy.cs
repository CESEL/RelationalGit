
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
    public class NothingKnowledgeShareStrategy : KnowledgeShareStrategy
    {

        public NothingKnowledgeShareStrategy(string knowledgeSaveReviewerReplacementType) : base(knowledgeSaveReviewerReplacementType)
        { }

        protected override PullRequestRecommendationResult RecommendReviewers(PullRequestContext pullRequestContext)
        {
            return new PullRequestRecommendationResult(new string[0]);
        }
    }
}
