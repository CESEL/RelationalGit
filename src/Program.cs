using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using AutoMapper;
using RelationalGit.Commands;
using LibGit2Sharp;
using System.Linq;
using RelationalGit.Mapping;
using Microsoft.EntityFrameworkCore;
using Octokit.Internal;
using Octokit;
using System.IO;
using System.Management.Automation;
using RelationalGit.CommandLine;
using CommandLine;

namespace RelationalGit
{
    class Program
    {
        static async Task Main(string[] args)
        {
            /*var inputLines = File.ReadAllLines(@"J:\vscode_commits.txt");
            var commitShas = inputLines.Select(q => q.Replace("\"", ""));
            var agentName = "mirsaeedi";
            var token = "c221f73fe7fd9806e10ffe6fe9f0cba2e12b650f";

            var credentials = new InMemoryCredentialStore(new Octokit.Credentials(token));
            var githubClient = new GitHubClient(new ProductHeaderValue(agentName), credentials);
            var outputLines = "";
            var commits = new List<GitHubCommit>();

            foreach (var commitSha in commitShas)
            {
                var commit= await githubClient
                    .Repository
                    .Commit
                    .Get("Microsoft", "vscode", commitSha);

                commits.Add(commit);
                outputLines += $"{commit.Sha},{commit.Author?.Login}" + Environment.NewLine;
            }

            File.WriteAllText("output.csv",outputLines);*/
            var userInput = new InputOption();

            Parser.Default.ParseArguments<InputOption>(args)
                .WithParsed(options=> userInput=options)
                .WithNotParsed(errors=>Console.WriteLine(errors.ToString()));
            
            using (var client = new GitRepositoryDbContext())
            {
                client.Database.Migrate();
            }

            ConfigMapping.Config();

            userInput = new InputOption()
             {
                 Command=CommandType.ExtractBlameForEachPeriod, 
                 RepositoryPath=@"E:\Repos\coreclr", 
                 GitBranch= "master", 
                 Extensions = ".cs,.vb,.ts,.js,.jsx,.sh,.yml,.tsx,.css,.json,.py,.c,.h,.cpp,.il,.make,.cmake,.ps1,.r,.cmd,.html,.conf".Split(',')
             };

            var arguments = Parser.Default.FormatCommandLine(userInput);


            await new CommandFactory().Execute(userInput);
        }

    }
}


