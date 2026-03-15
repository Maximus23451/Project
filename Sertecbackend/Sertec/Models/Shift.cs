using System.ComponentModel.DataAnnotations;

namespace Sertec.Models
{
    public class Shift
    {
        [Key]
        public int sId { get; set; }

        public int planned { get; set; }
        public int uph { get; set; }
        public int waste { get; set; }
        public int ups { get; set; }
        public int units { get; set; }
        public int unPlanned { get; set; }

    }
}
