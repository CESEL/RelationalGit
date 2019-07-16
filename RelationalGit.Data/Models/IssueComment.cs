using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace RelationalGit.Data
{
    public class IssueComment
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string UserLogin { get; set; }

        public long IssueNumber { get; set; }

        public DateTime CreatedAtDateTime { get; set; }

        public string Body { get; set; }

        public string HtmltUrl { get; set; }

        public string Url { get; set; }
        //public string AuthorAssociation { get; set; }
    }
}
