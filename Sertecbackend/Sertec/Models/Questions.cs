using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sertec.Models
{
    public class Questions
    {
        [Key]
        public int qid { get; set; }

        [ForeignKey("parts")]
        public int partsId { get; set; }

        public string question { get; set; }

        [ForeignKey("roles")]
        public int roleId { get; set; }

        public int frequency { get; set; }


        public virtual Parts parts { get; set; }

        public virtual Roles roles { get; set; }
    }
}
