namespace EmployeeManagementSystem.Domain.Services.Contracts
{
    public record LoginResponse(bool Flag, string Message = null!, string Token = null!, string RefreshToken = null!);

}
