using System.ComponentModel.DataAnnotations.Schema;

namespace Sertec.Models
{
    public class UserShifts
    {
        [ForeignKey("users")]
        public int userId { get; set; }

        [ForeignKey("shift")]
        public int shiftId { get; set; }

        public virtual Users users { get; set; }
        public virtual Shift shift { get; set; }

    }
}
