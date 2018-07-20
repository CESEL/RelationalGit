using System;
using System.Collections.Generic;
using System.Text;

namespace RelationalGit
{
    public class DeveloperKnowledge
    {
        public int NumberOfTouchedFiles { get; set; }
        public int NumberOfReviewedFiles { get; set; }
        public int NumberOfCommittedFiles { get; set; }
        public int NumberOfCommits { get; set; }
        public int NumberOfReviews { get; set; }
        public string DeveloperName { get; set; }
        public int NumberOfAuthoredLines { get; set; }
    }
}

