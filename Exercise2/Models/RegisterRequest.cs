using System.ComponentModel.DataAnnotations;

namespace EmployeeManagerment.Models
{
    public class RegisterRequest
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        public string Email { get; set; }

    }
}
