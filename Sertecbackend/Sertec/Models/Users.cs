using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sertec.Models
{
    public class Users
    {
        [Key]
        public int uid { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string? Email { get; set; }

        public string rfid { get; set; }

        [ForeignKey("RoleId")]
        public Roles roleid { get; set; }
    }
}
