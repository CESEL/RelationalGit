using System;
using System.ComponentModel.DataAnnotations;

namespace RelationalGit.Data
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

        public string Extension { get;  set; }

        public string FileType { get;  set; }

        public bool IsTest { get;  set; }
    }
}
