using EmployeeManagementSystem.Application.Contracts;
using EmployeeManagementSystem.Domain.DTOs;
using EmployeeManagementSystem.Application.Extension;
using EmployeeManagementSystem.Domain.Responses;
using EmployeeManagementSystem.Domain.Services.Contracts;
using EmployeeManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ServerLibrary.Repositories.Implementation
{
    public class UserAccountRepository(IOptions<JwtSection> config, AppDbContext appDbContext) : IUserAccountRepository
    {
        //Register Start Here ===============
        public async Task<GeneralResponse> CreateAsync(Register user)
        {
            //First CHeck if user is null
            if (user is null) return new GeneralResponse(false, "Model is Empty");
            //Then Check if user exists
            var checkUser = await FindUserByEmail(user.Email);
            if (checkUser != null) return new GeneralResponse(false, "User registered already");
            //We save the user
            var applicationUser = await AddToDatabase(new ApplicationUser()
            {
                FullName = user.FullName,
                Email = user.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(user.Password)
            });

            //Check and Asign role to the saved user
            //If there is no role, the first user should be an admin
            var checkAdminRole = await appDbContext.SystemRoles.FirstOrDefaultAsync(x => x.Name!.Equals(Constants.Admin));
            if (checkAdminRole is null)
            {
                var createAdminRole = await AddToDatabase(new SystemRole() { Name = Constants.Admin });
                await AddToDatabase(new UserRole() { RoleId = createAdminRole.Id, UserId = applicationUser.Id });
                return new GeneralResponse(true, "Account Created");
            }

            //After that we check
            var checkUserRole = await appDbContext.SystemRoles.FirstOrDefaultAsync(x => x.Name!.Equals(Constants.User));
            SystemRole response = new();
            if (checkUserRole is null)
            {
                response = await AddToDatabase(new SystemRole() { Name = Constants.User });
                await AddToDatabase(new UserRole() { RoleId = response.Id, UserId = applicationUser.Id });
            }
            else
            {
                await AddToDatabase(new UserRole() { RoleId = checkUserRole.Id, UserId = applicationUser.Id });
            }
            return new GeneralResponse(true, "Account Created");
        }
        //Register Stops Here ===============

        //Login Start Here ===============

        public async Task<LoginResponse> SignInAsync(Login user)
        {
            //Check if user is provides
            if (user is null) return new LoginResponse(false, "Model is empty");
            //Then Get the user
            var applicationUser = await FindUserByEmail(user.Email!);
            if (applicationUser is null) return new LoginResponse(false, "User not found");
            //Verify the password
            if (!BCrypt.Net.BCrypt.Verify(user.Password, applicationUser.Password)) return new LoginResponse(false, "Email/Password not valid");
            //If Password valid, we get the role
            //Get the user'a role
            var getUserRole = await FindUserRole(applicationUser.Id);
            if (getUserRole is null) return new LoginResponse(false, "User role not found1");
            //Get the role name
            var getRoleName = await FindRoleName(getUserRole.RoleId);
            if (getRoleName is null) return new LoginResponse(false, "User role name not found");
            //Then we generate the token for that user
            string jwtToken = GenerateToken(applicationUser, getRoleName!.Name!);
            string refreshToken = GenerateRefreshToken();
            //Save the refreshtoken to the RefreshToken Table
            //Save the refresh token to the database
            var findUser = await appDbContext.RefreshTokenInfos.FirstOrDefaultAsync(x => x.UserId == applicationUser.Id);
            if (findUser is not null)
            {
                findUser!.Token = refreshToken;
                await appDbContext.SaveChangesAsync();
            }
            else
            {
                await AddToDatabase(new RefreshTokenInfo() { Token = refreshToken, UserId = applicationUser.Id });
            }
            //Return the login response
            return new LoginResponse(true, "Login successfullt", jwtToken, refreshToken);
        }

        //Login Stops Here ===============

        //Refresh Token Start here =================
        public async Task<LoginResponse> RefreshTokenAsync(RefreshToken token)
        {
            //Check if the token is provided
            if (token == null) return new LoginResponse(false, "Model is Empty");
            //let find the token provided and check if tht exist in the table
            var findToken = await appDbContext.RefreshTokenInfos.FirstOrDefaultAsync(x => x.Token!.Equals(token.Token));
            if (findToken == null) return new LoginResponse(false, "Refresh token is required");
            //get the user's Id from the RefreshTokenInfo
            var user = await appDbContext.ApplicationUsers.FirstOrDefaultAsync(x => x.Id == findToken.UserId);
            if (user is null) return new LoginResponse(false, "Refresh token could not be generated because user not found");
            //Now We get the role, role name
            //get the role name
            var userRole = await FindUserRole(user.Id);
            var roleName = await FindRoleName(userRole.RoleId);
            string jwtToken = GenerateToken(user, roleName.Name!);
            string refreshToken = GenerateRefreshToken();
            //Check if the refresh token table contains that user info
            var updateRefreshToken = await appDbContext.RefreshTokenInfos.FirstOrDefaultAsync(x => x.UserId == user.Id);
            if (updateRefreshToken == null) return new LoginResponse(false, "Refresh token could not be generated because user has not signed in");
            //If it exists, we have to refresh or update the existing token to a new one
            updateRefreshToken.Token = refreshToken;
            await appDbContext.SaveChangesAsync();
            return new LoginResponse(true, "Token refreshed successfully", jwtToken, refreshToken);
        }
        //Refresh Token Stop here

        //Get user by Email
        private async Task<ApplicationUser> FindUserByEmail(string email) =>
            await appDbContext.ApplicationUsers.FirstOrDefaultAsync(x => x.Email!.ToLower().Equals(email!.ToLower()));

        //Get User Role
        private async Task<UserRole> FindUserRole(Guid userId) =>
            await appDbContext.UserRoles.FirstOrDefaultAsync(x => x.UserId == userId);
        //Get Role by name
        private async Task<SystemRole> FindRoleName(Guid roleId) =>
           await appDbContext.SystemRoles.FirstOrDefaultAsync(x => x.Id == roleId);

        //Save to the database
        private async Task<T> AddToDatabase<T>(T model)
        {
            var result = appDbContext.Add(model!);
            await appDbContext.SaveChangesAsync();
            return (T)result.Entity;
        }

        //Method to generate the token
        private string GenerateToken(ApplicationUser user, string role)
        {
            //Verify if the JwtSection is configured in the application.json
            if (config.Value.Key == null)
            {
                throw new Exception("JWT secret key is not configured.");
            }
            //Form the security key
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.Value.Key));
            //Make the credentials
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            //Create the user claims
            var userClaims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Role, role!)
            };
            // Create the token
            var token = new JwtSecurityToken(
                issuer: config.Value.Issuer,
                audience: config.Value.Audience,
                claims: userClaims,
                expires: DateTime.Now.AddDays(30),
                signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        //Method to generate the refresh token
        private static string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }
}
