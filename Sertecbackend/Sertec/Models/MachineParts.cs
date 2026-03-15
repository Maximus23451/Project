using System.ComponentModel.DataAnnotations.Schema;

namespace Sertec.Models
{
    public class MachineParts
    {
        [ForeignKey("Machines")]
        public int MachineId { get; set; }

        [ForeignKey("Parts")]
        public int PartId { get; set; }



        public virtual Machines Machines { get; set; }

        public virtual Parts Parts { get; set; }

    }
}
