using System;
using System.Collections.Generic;
using System.Text;

namespace RelationalGit
{
    public class FileBlame
    {
        public int TotalAuditedLines { get; set; }
        public string FileName { get; set; }
        public Period Period { get; set; }
        public string NormalizedDeveloperName { get; set; }
        public double OwnedPercentage { get; set; }
    }
}

