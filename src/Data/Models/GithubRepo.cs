using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RelationalGit
{
    public class GithubRepo
    {
        public int Id { get; set; }
        public string Owner { get; set; }
        public string Repo { get; set; }
    }
}
