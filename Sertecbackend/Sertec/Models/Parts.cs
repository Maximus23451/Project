using System.ComponentModel.DataAnnotations;

namespace Sertec.Models
{
    public class Parts
    {
        [Key]
        public int pid { get; set; }

        public string name { get; set; }

        public string serialNumber { get; set; }
    }
}
