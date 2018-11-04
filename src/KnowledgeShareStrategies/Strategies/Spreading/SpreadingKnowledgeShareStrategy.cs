
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
        protected override DeveloperKnowledge[] SortPRKnowledgeables(PullRequestContext pullRequestContext)
        {
            if (pullRequestContext.PRKnowledgeables.Length == 0)
                return pullRequestContext.PRKnowledgeables;
            /*
            //var maxTouchedFiles = pullRequestContext.PRKnowledgeables.Max(q=>q.NumberOfTouchedFiles);
            var maxTouchedFiles = pullRequestContext.PullRequestFiles.Count();

            var touchedFileUpperBound = Math.Ceiling(maxTouchedFiles * 0.5);
            var touchedFileLowerBound = Math.Ceiling(maxTouchedFiles * 0.0);

            var availableDevs = pullRequestContext.PRKnowledgeables.Where(q=>pullRequestContext.AvailableDevelopers.Any(d=>d.NormalizedName==q.DeveloperName));

            var lessKnowledgedDevelopers = availableDevs.Where(q=>q.NumberOfTouchedFiles<= touchedFileUpperBound && q.NumberOfTouchedFiles>= touchedFileLowerBound);

            if (lessKnowledgedDevelopers.Count() > 0)
            {
                return availableDevs.Where(q => q.NumberOfTouchedFiles > touchedFileUpperBound || q.NumberOfTouchedFiles<touchedFileUpperBound).OrderBy(q=>q.NumberOfTouchedFiles)
                    .Concat(lessKnowledgedDevelopers.OrderBy(q => q.NumberOfTouchedFiles).ThenBy(q => q.NumberOfReviews)).ToArray();
            }
            */

            return pullRequestContext.PRKnowledgeables.OrderByDescending(q=>q.NumberOfTouchedFiles).ThenByDescending(q=>q.NumberOfReviews).ToArray();
        }
    }
}
