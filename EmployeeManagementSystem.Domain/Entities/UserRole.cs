namespace EmployeeManagementSystem.Domain.Entities
{
    public class UserRole
    {
        public Guid Id { get; set; }
        public Guid RoleId { get; set; }
        public Guid UserId { get; set; }
    }
}
