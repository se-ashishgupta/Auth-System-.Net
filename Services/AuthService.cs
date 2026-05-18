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
        if(!await _db.Users.AnyAsync(u => u.Email == request.Email))
            throw new Exception("Invalid email or password.");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if(user == null)
            throw new Exception("Invalid email or password.");

        if(!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new Exception("Invalid email or password.");

        user.RefreshToken = _tokenService.GenerateRefreshToken();
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        await _db.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = _tokenService.GenerateAccessToken(user),
            RefreshToken = user.RefreshToken,
            Username = user.Username,
            Email = user.Email
        };
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);
        if(user == null)
            throw new Exception("Invalid refresh token.");

        if(user.RefreshTokenExpiry < DateTime.UtcNow)
            throw new Exception("Refresh token expired.");

        user.RefreshToken = _tokenService.GenerateRefreshToken();
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        await _db.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = _tokenService.GenerateAccessToken(user),
            RefreshToken = user.RefreshToken,
            Username = user.Username,
            Email = user.Email
        };
    }

    public async Task RevokeTokenAsync(RevokeTokenRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);
        if(user == null)
            throw new Exception("Invalid refresh token.");

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null; 

        await _db.SaveChangesAsync();
    }
}