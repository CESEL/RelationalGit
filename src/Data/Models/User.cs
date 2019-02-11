using System.ComponentModel.DataAnnotations;

namespace RelationalGit
{
    public class User
    {
        [Key]
        public string UserLogin { get; set; }

        public string Email { get; set; }

        public string Name { get; set; }
    }
}
