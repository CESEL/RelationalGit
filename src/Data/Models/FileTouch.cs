using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RelationalGit
{
    public class FileTouch
    {
        public long Id { get; set; }
        public string NormalizeDeveloperName { get; internal set; }
        public long PeriodId { get; internal set; }
        public string CanonicalPath { get; internal set; }
        public string TouchType { get; internal set; }
        public long LossSimulationId { get; internal set; }
    }
}
