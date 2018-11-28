
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
    public class MostTouchedFilesKnowlegdeShareStrategy : BaseKnowledgeShareStrategy
    {
        public MostTouchedFilesKnowlegdeShareStrategy(string knowledgeSaveReviewerReplacementType) : base(knowledgeSaveReviewerReplacementType)
        { }

        protected override DeveloperKnowledge[] SortCandidates(PullRequestContext pullRequestContext, DeveloperKnowledge[] candidates)
        {
            return candidates.OrderBy(q => q.NumberOfTouchedFiles).ToArray();
        }
    }
}