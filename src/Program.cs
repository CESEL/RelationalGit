using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using AutoMapper;
using RelationalGit.Commands;
using LibGit2Sharp;
using System.Linq;
using RelationalGit.Mapping;
using Microsoft.EntityFrameworkCore;

namespace RelationalGit
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using (var client = new GitRepositoryDbContext())
            {
                client.Database.Migrate();
            }

            ConfigMapping.Config();

           args = new string[]
           {
               "-get-merge-events", // command
               "618196d41ee2db3e6879ce8899300d1aa9c6fb53", //token
               "mirsaeedi", // agent name
               "jeremy091", // owner
               "refugeeAI", // repo
               "dev" // branch
           };

           /*args = new string[]
           {
               "-get-git-blobsblames-for-periods", // command
               @"C:\Users\Ehsan Mirsaeedi\Desktop\Repo\refugeeAI", //token
               "dev",
               ".cs,.vb,.ts,.js,.jsx,.sh,.yml"// agent name
           };*/

            /*args = new string[]
            {
                "-periodize-git-commits", // command
                @"C:\Users\Ehsan Mirsaeedi\Desktop\Repo\refugeeAI", //token
                "dev", // agent name
            };*/

            /*args = new string[]
            {
                "-get-git-commitsChanges", // command
                @"C:\Users\Ehsan Mirsaeedi\Desktop\Repo\refugeeAI", //token
                "dev", // agent name
            };*/


            /*args = new string[]
            {
                "-get-git-commits", // command
                @"C:\Users\Ehsan Mirsaeedi\Desktop\Repo\refugeeAI", //token
                "dev", // agent name
            };*/


            await new CommandFactory().Execute(args);
        }

    }
}


