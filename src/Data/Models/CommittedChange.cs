using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RelationalGit
{
    public class CommittedChange
    {
        public CommittedChange()
        {
            Id = Guid.NewGuid();
        }

        [Key]
        public Guid Id { get; set; }
        public string Oid { get; set; }
        public string Path { get; set; }
        public short Status { get; set; }
        public string CommitSha { get; set; }
        public string CanonicalPath { get; set; }
        public string OldPath { get; set; }
        public string OldOid { get; set; }
    }
}
