using System.ComponentModel.DataAnnotations;


namespace SertecDashboard.Api.Models
{
    public class Parts
    {
        [Key]
        public int partId { get; set; }
        public string Name { get; set; }
        public string serialNumber { get; set; }


    }
}
