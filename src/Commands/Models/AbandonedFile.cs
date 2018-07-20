using System;
using System.Collections.Generic;
using System.Text;

namespace RelationalGit
{
    public class AbandonedFile
    {
        public string FilePath { get; set; }         
        public int TotalLinesInPeriod { get; set; }
        public int AbandonedLinesInPeriod { get; set; }
        public int SavedLines =>TotalLinesInPeriod-AbandonedLinesInPeriod;
    }
}

