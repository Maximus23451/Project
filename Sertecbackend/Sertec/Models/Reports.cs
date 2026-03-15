using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sertec.Models
{
    public class Reports
    {
        [Key]
        public int reportId { get; set; }

        [ForeignKey("questions")]
        public Questions qId { get; set; }

        public string report { get; set; }
        public DateTime reportCreated { get; set; }

        [ForeignKey("users")]
        public Users uId { get; set; }

    }
}
