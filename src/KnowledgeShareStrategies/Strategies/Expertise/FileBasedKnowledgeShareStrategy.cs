
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
    public class FileBasedKnowledgeShareStrategy : BaseKnowledgeShareStrategy
    {
        protected override IEnumerable<DeveloperKnowledge> SortDevelopersKnowledge(DeveloperKnowledge[] developerKnowledges,PullRequestContext pullRequestContext)
        {
            var presentDevs  = developerKnowledges
            .Where(q=>pullRequestContext
            .availableDevelopers.Any(d=>d.NormalizedName==q.DeveloperName));

            return presentDevs
            .OrderBy(q => q.NumberOfAuthoredLines)
            .ThenBy(q => q.NumberOfCommits)
            .ThenBy(q=>q.NumberOfReviews);
        }
    }
}