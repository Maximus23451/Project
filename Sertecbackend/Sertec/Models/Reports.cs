using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sertec.Models
{
    public class Reports
    {
        [Key]
        public int reportId { get; set; }

        [ForeignKey("questions")]
        public int qId { get; set; }

        public short report { get; set; }
        public DateTime reportCreated { get; set; }

        public string explanation { get; set; }

        [ForeignKey("users")]
        public int uId { get; set; }

        public virtual Users users { get; set; }

        public virtual Questions questions { get; set; }

    }
}
