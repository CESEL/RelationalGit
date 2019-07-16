using System.Linq;

namespace RelationalGit.Data
{
    public class UsernameRepository
    {
        private readonly GitHubGitUser[] _githubGitUsers;
        private readonly Developer[] _developers;

        public UsernameRepository(GitHubGitUser[] githubGitUsers, Developer[] developers)
        {
            _githubGitUsers = githubGitUsers;
            _developers = developers;
        }

        public Developer GetByGitHubLogin(string userLogin)
        {
            var normalizedName = _githubGitUsers.FirstOrDefault(q => q.GitHubUsername == userLogin)?.GitNormalizedUsername;

            // we have ignored some of the mega developers
            return _developers.SingleOrDefault(q => q.NormalizedName == normalizedName || q.NormalizedName == "UnmatchedGithubLogin-" + userLogin);
        }
    }
}
