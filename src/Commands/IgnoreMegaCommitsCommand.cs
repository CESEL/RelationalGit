
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Diacritics.Extensions;
using F23.StringSimilarity;

namespace RelationalGit.Commands
{
    public class IgnoreMegaCommitsCommand
    {
        public async Task Execute(int megaCommitSize)
        {
            using (var dbContext = new GitRepositoryDbContext())
            {
                await dbContext.Database.ExecuteSqlCommandAsync($@"
                UPDATE Commits set Ignore=0;
                UPDATE Commits set Ignore=1
                WHERE Sha in (select CommitSha from CommittedChanges group by CommitSha having count(*)>={megaCommitSize})
                UPDATE CommitBlobBlames set Ignore=0;
                UPDATE CommitBlobBlames set Ignore=1
                WHERE AuthorCommitSha in (select CommitSha from CommittedChanges group by CommitSha having count(*)>={megaCommitSize})");
            }  
        }
    }
}
