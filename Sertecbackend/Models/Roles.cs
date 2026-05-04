using System.ComponentModel.DataAnnotations;


namespace SertecDashboard.Api.Models
{
    public class Roles
    {
        [Key]
        public int roleId { get; set; }
        public string Name { get; set; }

    }
}
