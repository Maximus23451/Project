using System.ComponentModel.DataAnnotations;

namespace Sertec.Models
{
    public class Machines
    {
        [Key]
        public int machineId { get; set; }
        public string name { get; set; }

    }
}
