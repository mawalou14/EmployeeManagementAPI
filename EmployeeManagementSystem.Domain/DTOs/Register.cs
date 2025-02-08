using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementSystem.Domain.DTOs
{
    public class Register : AccountBase
    {
        [Required(ErrorMessage = "Full name is required.")]
        [MinLength(5, ErrorMessage = "Must be at least 5 characters long.")]
        [MaxLength(100, ErrorMessage = "Cannot be longer than 100 characters.")]
        public string? FullName { get; set; }
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Confirm password is required.")]
        [Compare(nameof(Password), ErrorMessage = "Must match with the password above.")]
        public string? ConfirmPassword { get; set; }
    }
}
