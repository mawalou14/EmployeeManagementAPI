namespace EmployeeManagementSystem.Domain.Entities
{
    public class RefreshTokenInfo
    {
        public Guid Id { get; set; }
        public string? Token { get; set; }
        public Guid UserId { get; set; }
    }
}
