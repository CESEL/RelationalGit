using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RelationalGit
{
    public class CommitBlobBlame
    {
        public string CommitSha { get; set; }
        public string CanonicalPath { get; set; }
        public string DeveloperIdentity { get; set; }
        public int AuditedLines { get; set; }
        public double AuditedPercentage { get; set; }
    }
}
