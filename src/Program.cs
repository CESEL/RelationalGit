using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using AutoMapper;
using RelationalGit.Commands;
using LibGit2Sharp;
using System.Linq;
using RelationalGit.Mapping;

namespace RelationalGit
{
    class Program
    {
        static async Task Main(string[] args)
        {
            ConfigMapping.Config();
            await new CommandFactory().Execute(args);

            args = new string[]
            {
                "-get-pullrequests-files", // command
                "618196d41ee2db3e6879ce8899300d1aa9c6fb53", //token
                "mirsaeedi", // agent name
                "dotnet", // owner
                "roslyn", // repo
                "master" // branch
            };

            args = new string[]
            {
                "-get-git-commits", // command
                @"C:\Users\Ehsan Mirsaeedi\Desktop\Repos\Roslyn", //token
                "master", // agent name
            };
        }

    }
}


