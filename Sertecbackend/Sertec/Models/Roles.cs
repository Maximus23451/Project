using System.ComponentModel.DataAnnotations;

namespace Sertec.Models
{
    public class Roles
    {
        [Key]
        public int Rid { get; set; }

        public string Name { get; set; }
    }
}
