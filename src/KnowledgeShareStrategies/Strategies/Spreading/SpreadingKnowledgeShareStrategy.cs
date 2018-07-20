
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
    public class SpreadingKnowledgeShareStrategy  : BaseKnowledgeShareStrategy
    {
        protected override IEnumerable<DeveloperKnowledge> SortDevelopersKnowledge(DeveloperKnowledge[] developerKnowledges,PullRequestContext pullRequestContext)
        {

            return developerKnowledges.OrderBy(q=>q.NumberOfTouchedFiles);

            var maxTouchedFiles=developerKnowledges.Max(q=>q.NumberOfTouchedFiles);

            var presentDevs  = developerKnowledges
            .Where(q=>pullRequestContext
            .availableDevelopers.Any(d=>d.NormalizedName==q.DeveloperName));

            var lessKnowledgedDevelopers= presentDevs
            .Where(q=>q.NumberOfTouchedFiles<=maxTouchedFiles*(0.66));

            if(lessKnowledgedDevelopers.Count()>0)
                return lessKnowledgedDevelopers.OrderBy(q => q.NumberOfCommits).ThenBy(q=>q.NumberOfTouchedFiles);

            return lessKnowledgedDevelopers.OrderBy(q=>q.NumberOfCommits).ThenBy(q=>q.NumberOfTouchedFiles);
        }
    }
}
