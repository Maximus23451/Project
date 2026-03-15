using System.ComponentModel.DataAnnotations.Schema;

namespace Sertec.Models
{
    public class ShiftParts
    {
        [ForeignKey("shift")]
        public int shiftId { get; set; }

        [ForeignKey("parts")]
        public int partId { get; set; }

        public virtual Shift shift { get; set; }
        public virtual Parts parts { get; set; }

    }
}
