using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementSystem.Domain.DTOs
{
    public class AccountBase
    {
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "The email address is not valid.")]
        [Required(ErrorMessage = "Email is required.")]
        public string? Email { get; set; }
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Must be at least 6 characters long.")]
        public string? Password { get; set; }
    }
}
