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
using Microsoft.Extensions.Logging;

namespace RelationalGit
{
    class Program
    {
        static async Task Main(string[] args)
        {
            ILogger logger = GetLogger();

            ConfigMapping.Config();

            var userInput = new InputOption();

            /*Parser.Default.ParseArguments<InputOption>(args)
                .WithParsed(options=> userInput=options)
                .WithNotParsed(errors=>Console.WriteLine(errors.ToString()));*/

            //InitDatabase();

            userInput = new InputOption()
            {
                Command = CommandType.ComputeKnowledgeLoss,
                KnowledgeSaveStrategyType = KnowledgeShareStrategyType.Ideal,
                CoreDeveloperCalculationType=CoreDeveloperCalculationType.AuthoredLines,
                LeaversType = LeaversType.All,
                CoreDeveloperThreshold = 15000,
                MegaPullRequestSize = 100,
                FilesAtRiksOwnershipThreshold=0.90,
                LeaversOfPeriodExtendedAbsence = 4,
                FilesAtRiksOwnersThreshold=1,
                MegaCommitSize = 200,
                MegaDevelopers = new []{"dotnetbot"},//,"dotnet-maestro-bot",
                PeriodType = PeriodType.Month,
                PeriodLength = 3,
                GitHubOwner = "dotnet",
                GitHubToken = "f1bf8a0d539b6aa8724e1921dd8f89b5b1785fa8",
                GitHubRepo = "corefx",
                RepositoryPath = @"/home/ehsan/Documents/Repositories/coreclr",
                GitBranch = "master",
                Extensions = ".cs,.vb,.ts,.js,.jsx,.sh,.yml,.tsx,.css,.json,.py,.c,.h,.cpp,.il,.make,.cmake,.ps1,.r,.cmd,.html,.conf".Split(',')
            };

            var arguments = Parser.Default.FormatCommandLine(userInput);

            await new CommandFactory().Execute(userInput, logger);
        }

        private static ILogger GetLogger()
        {
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddConsole().AddDebug();
            var logger = loggerFactory.CreateLogger("Github.Octokit");
            return logger;
        }

        private static void InitDatabase()
        {
            using (var client = new GitRepositoryDbContext())
            {
                client.Database.EnsureCreated();
            }
        }
    }
}


