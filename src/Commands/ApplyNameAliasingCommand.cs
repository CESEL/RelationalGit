
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
    public class ApplyNameAliasingCommand
    {
        public async Task Execute()
        {
            using (var dbContext = new GitRepositoryDbContext(false,commandTimeout:150000))
            {
               await dbContext
               .Database
               .ExecuteSqlCommandAsync(
                   @"UPDATE Commits SET NormalizedAuthorName=AliasedDeveloperNames.NormalizedName
                    from AliasedDeveloperNames
                    where AliasedDeveloperNames.Email=Commits.AuthorEmail");

                await dbContext
                .Database
               .ExecuteSqlCommandAsync(
                   @"UPDATE CommitBlobBlames SET NormalizedDeveloperIdentity=AliasedDeveloperNames.NormalizedName
                    from AliasedDeveloperNames
                    where AliasedDeveloperNames.Email=CommitBlobBlames.DeveloperIdentity");
            }
        }
    }
}
