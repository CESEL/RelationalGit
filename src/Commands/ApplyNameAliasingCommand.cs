
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RelationalGit.Commands
{
    public class ApplyNameAliasingCommand
    {
        private readonly ILogger _logger;

        public ApplyNameAliasingCommand(ILogger logger)
        {
            _logger = logger;
        }

        public async Task Execute()
        {
            using (var dbContext = new GitRepositoryDbContext(false))
            {
                _logger.LogInformation("{datetime}: applying normalization on commits.", DateTime.Now);

                await dbContext
               .Database
               .ExecuteSqlCommandAsync(
                   @"UPDATE Commits SET NormalizedAuthorName=AliasedDeveloperNames.NormalizedName
                    from AliasedDeveloperNames
                    where AliasedDeveloperNames.Email=Commits.AuthorEmail");

                _logger.LogInformation("{datetime}: applying normalization on committed blob blames.", DateTime.Now);

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
