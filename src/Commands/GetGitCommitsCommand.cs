﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationalGit.Commands
{
    public class GetGitCommitsCommand
    {
        public async Task Execute(string repoPath, string branchName)
        {

            using (var dbContext = new GitRepositoryDbContext())
            {
                var gitRepository = new GitRepository(repoPath);
                var orderedCommits = gitRepository.ExtractCommitsFromBranch(branchName);
                dbContext.Commits.AddRange(orderedCommits);
                var relationships = orderedCommits.SelectMany(q => q.CommitRelationship);
                dbContext.CommitRelationships.AddRange(relationships);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
