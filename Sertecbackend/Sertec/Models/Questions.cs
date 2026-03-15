using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sertec.Models
{
    public class Questions
    {
        [Key]
        public int qid { get; set; }

        [ForeignKey("parts")]
        public Parts partsId { get; set; }

        public string question { get; set; }

        [ForeignKey("roles")]
        public Roles roleId { get; set; }
    }
}
