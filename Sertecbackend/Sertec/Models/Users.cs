using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sertec.Models
{
    public class Users
    {
        [Key]
        public int uid { get; set; }

        public string Username { get; set; }

        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }

        public string? Email { get; set; }

        public string rfid { get; set; }

        [ForeignKey("RoleId")]
        public int roleid { get; set; }
    }
}
