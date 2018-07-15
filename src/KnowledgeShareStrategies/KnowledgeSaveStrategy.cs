
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
    public abstract class RecommendingReviewersKnowledgeShareStrategy
    {
        public static RecommendingReviewersKnowledgeShareStrategy Create(string knowledgeShareStrategyType)
        {
            if (knowledgeShareStrategyType == KnowledgeShareStrategyType.Nothing)
            {
                return new NothingKnowledgeShareStrategy();
            }
            if (knowledgeShareStrategyType == KnowledgeShareStrategyType.ActualReviewers)
            {
                return new ActualKnowledgeShareStrategy();
            }
            if (knowledgeShareStrategyType == KnowledgeShareStrategyType.Expertise)
            {
                return new ExpertiseBasedKnowledgeShareStrategy();
            }
            if (knowledgeShareStrategyType == KnowledgeShareStrategyType.Ideal)
            {
                return new IdealKnowledgeShareStrategy();
            }
            if (knowledgeShareStrategyType == KnowledgeShareStrategyType.CommitBasedExpertiseReviewers)
            {
                return new ExpertiseCommitBasedKnowledgeShareStrategy();
            }
            if (knowledgeShareStrategyType == KnowledgeShareStrategyType.CommitBasedSpreadingReviewers)
            {
                return new CommitBasedSpreadingKnowledgeShareStrategy();
            }

            throw new ArgumentException($"invalid {nameof(knowledgeShareStrategyType)}");
        }
        internal abstract string[] RecommendReviewers(PullRequestContext pullRequestContext);
    }
}
