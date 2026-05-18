using AuthSystem.Data;
using AuthSystem.DTOs;
using AuthSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthSystem.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly ITokenService _tokenService;

    public AuthService(AppDbContext db, ITokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Check if email already exists
        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            throw new Exception("Email already in use.");

        // Check if username already exists
        if (await _db.Users.AnyAsync(u => u.Username == request.Username))
            throw new Exception("Username already taken.");

        // Create user
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        // Generate tokens
        user.RefreshToken = _tokenService.GenerateRefreshToken();
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = _tokenService.GenerateAccessToken(user),
            RefreshToken = user.RefreshToken,
            Username = user.Username,
            Email = user.Email
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        throw new NotImplementedException(); // Step 5
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        throw new NotImplementedException(); // Step 6
    }

    public async Task RevokeTokenAsync(string refreshToken)
    {
        throw new NotImplementedException(); // Step 7
    }
}