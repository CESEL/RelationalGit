using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Diacritics.Extensions;
using F23.StringSimilarity;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace RelationalGit.Commands
{
    public class AliasGitNamesCommand
    {
        private ILogger _logger;

        public AliasGitNamesCommand(ILogger logger)
        {
            _logger = logger;
        }

        public async Task Execute()
        {
            using (var dbContext = new GitRepositoryDbContext(false))
            {
                var normalizedDevelopers = new List<AliasedDeveloperName>();

                var authorsPlace = new Dictionary<string, string>();

                var authors = dbContext.Commits
                    .Select(m => new { m.AuthorEmail, m.AuthorName })
                    .Distinct()
                    .ToArray();

                _logger.LogInformation("{datetime}: there are {count} authors submitted all the commits.", DateTime.Now, authors.Count());

                foreach (var author in authors)
                {
                    var normalizedEmail = author.AuthorEmail
                        .Replace(" ",string.Empty)
                        .ToLower()
                        .Trim()
                        .RemoveDiacritics();
                    
                    var normalizedName = author.AuthorName
                        .Replace(" ",string.Empty)
                        .Trim()
                        .ToLower()
                        .RemoveDiacritics();

                    // we remove () [] '' "" and all the text in between
                    string regex = "(\\[.*\\])|(\".*\")|('.*')|(\\(.*\\))";
                    normalizedName = Regex.Replace(normalizedName, regex,string.Empty);
                    
                    if (authorsPlace.ContainsKey(normalizedName))
                    {
                        var uniqueId = authorsPlace[normalizedName];

                        if (authorsPlace.ContainsKey(normalizedEmail) &&
                            authorsPlace[normalizedEmail] != uniqueId)
                        {
                            var oldUniqueId = authorsPlace[normalizedEmail];
                            
                            foreach(var dev in normalizedDevelopers.Where(q => q.NormalizedName == oldUniqueId))
                            {
                                dev.NormalizedName = uniqueId;
                            }
                        }

                        authorsPlace[normalizedEmail] = uniqueId;
                    }
                    else if (authorsPlace.ContainsKey(normalizedEmail))
                    {
                        var uniqueId = authorsPlace[normalizedEmail];
                        authorsPlace[normalizedName] = uniqueId;
                    }
                    else
                    {
                        authorsPlace[normalizedName] = normalizedName;
                        authorsPlace[normalizedEmail] = normalizedName;
                    }

                    normalizedDevelopers.Add(new AliasedDeveloperName(){
                        Email = author.AuthorEmail,
                        Name = author.AuthorName,
                        NormalizedName = authorsPlace[normalizedName]
                    });
                }

                var damerauDistanceAlgorithm = new Damerau();
                normalizedDevelopers = normalizedDevelopers.OrderBy(q => q.NormalizedName)
                .ToList();

                for(var i = 0;i < normalizedDevelopers.Count - 1;i++)
                {
                    var firstDev = normalizedDevelopers[i];
                    var secondDev = normalizedDevelopers[i + 1];
                    var distance = damerauDistanceAlgorithm.Distance(firstDev.NormalizedName, secondDev.NormalizedName);
                    
                    if(distance == 1)
                    {
                        secondDev.NormalizedName = firstDev.NormalizedName;        
                    }
                }

                _logger.LogInformation("{datetime}: after normalization, there are {count} unique authors have been found.", 
                    DateTime.Now, normalizedDevelopers.Select(q => q.NormalizedName).Distinct().Count());


                dbContext.AddRange(normalizedDevelopers);
                await dbContext.SaveChangesAsync();

                _logger.LogInformation("{datetime}: aliased results have been saves successfully.",DateTime.Now);
            }
        }
    }
}
