using System.ComponentModel.DataAnnotations;


namespace Sertec.Models
{
    public class PasswordRequest
    {

        public int userId { get; set; }
        public DateTime requestedAt { get; set; }


    }
}
