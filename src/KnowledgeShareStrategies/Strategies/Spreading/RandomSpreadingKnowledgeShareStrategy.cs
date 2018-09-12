
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
    public class RandomSpreadingKnowledgeShareStrategy : BaseKnowledgeShareStrategy
    {
        private Random _random = new Random();
        protected override IEnumerable<DeveloperKnowledge> SortDevelopersKnowledge(DeveloperKnowledge[] developerKnowledges, PullRequestContext pullRequestContext)
        {

            if (developerKnowledges.Length == 0)
                return developerKnowledges;

            var sortedDeveloperKnowledges = developerKnowledges
                .OrderBy(q => q.NumberOfReviews)
                .OrderBy(q => q.NumberOfReviewedFiles)
                .OrderBy(q => q.NumberOfCommits)
                .ToList();

            var availableDevelopers = pullRequestContext.AvailableDevelopers.Where(q=>q.TotalCommits>10 || q.TotalReviews>10).Select(q => q.NormalizedName);
            var experiencedDevelopers = developerKnowledges.Select(q => q.DeveloperName);
            var nonexperiencedDevelopers = availableDevelopers.Except(experiencedDevelopers).ToArray();

            if (nonexperiencedDevelopers.Length == 0)
                return sortedDeveloperKnowledges;

            var randomDeveloper = nonexperiencedDevelopers[_random.Next(0, nonexperiencedDevelopers.Length)];

            sortedDeveloperKnowledges.Add(new DeveloperKnowledge()
            {
                DeveloperName=randomDeveloper
            });

            return sortedDeveloperKnowledges;
        }
    }
}
