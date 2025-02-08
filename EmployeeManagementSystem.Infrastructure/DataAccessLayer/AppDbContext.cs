using EmployeeManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace EmployeeManagementSystem.Infrastructure.Data
{
    public class AppDbContext (DbContextOptions options): DbContext(options)
    {

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<SystemRole> SystemRoles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RefreshTokenInfo> RefreshTokenInfos { get; set; }
    }
}
