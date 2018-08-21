﻿
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
    public class BlameBasedKnowledgeShareStrategy : BaseKnowledgeShareStrategy
    {
        protected override IEnumerable<DeveloperKnowledge> SortDevelopersKnowledge(DeveloperKnowledge[] developerKnowledges,PullRequestContext pullRequestContext)
        {
            return developerKnowledges
            .OrderBy(q => q.NumberOfAuthoredLines)
            .ThenBy(q => q.NumberOfCommits)
            .ThenBy(q=>q.NumberOfCommittedFiles);
        }
    }
}