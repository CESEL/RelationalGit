using System;
using System.Collections.Generic;
using System.Text;

namespace RelationalGit
{
    public class GitHubGitUser
    {
        public long Id { get; set; }
        public string GitUsername { get; set; }
        public string GitNormalizedUsername { get; set; }
        public string GitHubUsername { get; set; }
    }
}
