
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RelationalGit.Data;

namespace RelationalGit.Commands
{
    public class IgnoreMegaCommitsCommand
    {
        private readonly ILogger _logger;

        public IgnoreMegaCommitsCommand(ILogger logger)
        {
            _logger = logger;
        }

        public async Task Execute(int megaCommitSize, IEnumerable<string> developerNames)
        {
            var developerNamesSet = "( " + developerNames
            .Select(q => $"'{q}'")
            .Aggregate((a, b) => $"{a},{b}") + " )";

            using (var dbContext = new GitRepositoryDbContext(false))
            {
                var query = $@"
                UPDATE Commits set Ignore=0;
                UPDATE Commits set Ignore=1
                WHERE NormalizedAuthorName in {developerNamesSet}
                OR Sha in (select CommitSha from CommittedChanges group by CommitSha having count(*)>={megaCommitSize});
                UPDATE CommitBlobBlames set Ignore=0;
                UPDATE CommitBlobBlames set Ignore=1
                WHERE NormalizedDeveloperIdentity in {developerNamesSet};
                UPDATE CommitBlobBlames set Ignore=1
                FROM CommitBlobBlames
                INNER JOIN (select CommitSha from CommittedChanges group by CommitSha having count(*)>={megaCommitSize}) as t
                On AuthorCommitSha=t.CommitSha";

                await dbContext.Database.ExecuteSqlCommandAsync(query).ConfigureAwait(false);
            }
        }
    }
}
