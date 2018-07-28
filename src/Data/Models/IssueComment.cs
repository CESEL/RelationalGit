﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RelationalGit
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
    }
}
