using System.ComponentModel.DataAnnotations.Schema;

namespace RelationalGit.Data
{
    public class PullRequestReviewer
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        public string UserLogin { get; set; }

        public string CommitId { get; internal set; }

        public string State { get; internal set; }

        public long PullRequestNumber { get; set; }
        //public string AuthorAssociation { get; set; }
    }
}
