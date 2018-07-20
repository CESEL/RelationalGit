using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelationalGit
{
    public class PeriodReviewerCountQuery
    {
        public long PeriodId { get; set; }
        public string GitHubUserLogin { get; set; }
        public int Count { get; set; }
    }
}
