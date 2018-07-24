
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
    public class RealisticIdealKnowledgeShareStrategy : KnowledgeShareStrategy
    {
        internal override string[] RecommendReviewers(PullRequestContext pullRequestContext)
        {
            if (pullRequestContext.ActualReviewers.Count() == 0)
                return new string[0];
                
            var oldestDevelopers = pullRequestContext.Developers
            .Values
            .Where(q=>q.FirstCommitPeriodId<=pullRequestContext.Period.Id);

            var longtermStayedDeveloper = oldestDevelopers
            .OrderBy(q=>q.LastCommitPeriodId-q.FirstCommitPeriodId).Last();

            return new string[]{longtermStayedDeveloper.NormalizedName};
        }
    }
}
