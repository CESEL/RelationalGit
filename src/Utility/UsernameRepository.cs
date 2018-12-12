using System.Linq;

namespace RelationalGit
{
    public class UsernameRepository
    {
        private GitHubGitUser[] _githubGitUsers;
        private Developer[] _developers;

        public UsernameRepository(GitHubGitUser[] githubGitUsers, Developer[] developers)
        {
            _githubGitUsers = githubGitUsers;
            _developers = developers;
        }

        internal Developer GetByGitHubLogin(string userLogin)
        {
            var normalizedName = _githubGitUsers.FirstOrDefault(q => q.GitHubUsername == userLogin)?.GitNormalizedUsername;

            // we have ignored some of the mega developers
            return _developers.SingleOrDefault(q => q.NormalizedName == normalizedName || q.NormalizedName == "UnmatchedGithubLogin-" + userLogin);
        }
    }
}
