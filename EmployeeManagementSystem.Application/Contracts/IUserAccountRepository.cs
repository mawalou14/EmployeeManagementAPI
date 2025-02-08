using EmployeeManagementSystem.Domain.DTOs;
using EmployeeManagementSystem.Domain.Responses;
using EmployeeManagementSystem.Domain.Services.Contracts;

namespace EmployeeManagementSystem.Application.Contracts
{
    public interface IUserAccountRepository
    {
        Task<GeneralResponse> CreateAsync(Register user);
        Task<LoginResponse> SignInAsync(Login user);
        Task<LoginResponse> RefreshTokenAsync(RefreshToken token);

    }
}
