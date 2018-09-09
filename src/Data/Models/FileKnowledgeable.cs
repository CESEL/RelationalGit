using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RelationalGit
{
    public class FileKnowledgeable
    {
        public long Id { get; set; }
        public long PeriodId { get; internal set; }
        public string CanonicalPath { get; internal set; }
        public int TotalKnowledgeables { get; internal set; }
        public long LossSimulationId { get; internal set; }
        public string Knowledgeables { get; internal set; }
    }
}
