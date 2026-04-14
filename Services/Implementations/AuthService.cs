using TestManagementApplication.Helpers;
using TestManagementApplication.Models.DTOs.Auth;
using TestManagementApplication.Models.Entities;
using TestManagementApplication.Repositories.Interfaces;
using TestManagementApplication.Services.Interfaces;

namespace TestManagementApplication.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly JwtHelper _jwtHelper;
        private readonly ITokenService _tokenService;

        public AuthService(IUserRepository userRepo, JwtHelper jwtHelper, ITokenService tokenService)
        {
            _userRepo = userRepo;
            _jwtHelper = jwtHelper;
            _tokenService = tokenService;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username))
                throw new ArgumentException("Username is required.");
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("Email is required.");
            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
                throw new ArgumentException("Password must be at least 6 characters.");

            if (await _userRepo.GetByUsernameAsync(request.Username) != null)
                throw new InvalidOperationException("Username already taken.");
            if (await _userRepo.GetByEmailAsync(request.Email) != null)
                throw new InvalidOperationException("Email already registered.");

            var refreshToken = _tokenService.GenerateRefreshToken();

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = "User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                RefreshToken = refreshToken,
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7)
            };

            await _userRepo.AddAsync(user);

            var token = _jwtHelper.GenerateToken(user);
            return new AuthResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("Username and password are required.");

            var user = await _userRepo.GetByUsernameAsync(request.Username)
                ?? throw new UnauthorizedAccessException("Invalid credentials.");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Your account has been deactivated.");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid credentials.");

            var accessToken = _jwtHelper.GenerateToken(user); // JWT
            var refreshToken = _tokenService.GenerateRefreshToken(); // random string

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _userRepo.UpdateAsync(user);
            return new AuthResponse
            {
                Token = accessToken,
                RefreshToken = refreshToken,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };
        }

        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AccessToken) || string.IsNullOrWhiteSpace(request.RefreshToken))
                throw new ArgumentException("Access token and refresh token are required.");

            // Extract user identity from the expired access token
            var principal = _jwtHelper.GetPrincipalFromExpiredToken(request.AccessToken);
            var userId = JwtHelper.GetUserIdFromClaims(principal);

            var user = await _userRepo.GetByIdAsync(userId)
                ?? throw new UnauthorizedAccessException("User not found.");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Your account has been deactivated.");

            // Validate the refresh token matches and hasn't expired
            if (user.RefreshToken != request.RefreshToken)
                throw new UnauthorizedAccessException("Invalid refresh token.");

            if (user.RefreshTokenExpiryTime == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                throw new UnauthorizedAccessException("Refresh token has expired. Please login again.");

            // Issue new tokens (token rotation)
            var newAccessToken = _jwtHelper.GenerateToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _userRepo.UpdateAsync(user);

            return new AuthResponse
            {
                Token = newAccessToken,
                RefreshToken = newRefreshToken,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };
        }

        public async Task RevokeTokenAsync(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
                throw new ArgumentException("Access token is required.");

            var principal = _jwtHelper.GetPrincipalFromExpiredToken(accessToken);
            var userId = JwtHelper.GetUserIdFromClaims(principal);

            var user = await _userRepo.GetByIdAsync(userId)
                ?? throw new UnauthorizedAccessException("User not found.");

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;

            await _userRepo.UpdateAsync(user);
        }
    }
}
