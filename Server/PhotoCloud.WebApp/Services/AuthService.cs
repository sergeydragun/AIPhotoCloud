using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using Infractructure.DTO.WebAppClientDTO;
using Infrastructure.Data.Entities;
using Infrastructure.Data.Interfaces;
using Infrastructure.Data.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PhotoCloud.Configuration;
using PhotoCloud.Interfaces;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserCredentialsRepository _credentialsRepository;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly JwtSettings _jwtSettings;
    private readonly IServiceProvider _serviceProvider;

    public AuthService(
        IUserRepository userRepository,
        IUserCredentialsRepository credentialsRepository,
        IPasswordHasher<User> passwordHasher,
        IOptions<JwtSettings> jwtOptions,
        IServiceProvider serviceProvider)
    {
        _userRepository = userRepository;
        _credentialsRepository = credentialsRepository;
        _passwordHasher = passwordHasher;
        _jwtSettings = jwtOptions.Value;
        _serviceProvider = serviceProvider;
    }

    public async Task<AuthResult> RegisterAsync(RegisterDto dto)
    {
        if (dto == null) return new AuthResult(false, null, null, "Payload is null");

        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password != dto.ConfirmPassword)
            return new AuthResult(false, null, null, "Passwords missing or do not match");

        var email = dto.Email?.Trim().ToLowerInvariant();
        var username = dto.UserName?.Trim();
        string? phoneE164 = null;
        if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
        {
            if (!TryNormalizePhoneToE164(dto.PhoneNumber!, out var phoneNorm))
                return new AuthResult(false, null, null, "Invalid phone format");
            phoneE164 = phoneNorm;
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            var sameEmail = await _userRepository.Find(u => u.Email.ToLower() == email).AnyAsync();
            if (sameEmail) return new AuthResult(false, null, null, "Email already in use");
        }

        if (!string.IsNullOrWhiteSpace(username))
        {
            var sameUser = await _userRepository.Find(u => u.UserName.ToLower() == username.ToLower()).AnyAsync();
            if (sameUser) return new AuthResult(false, null, null, "Username already in use");
        }

        if (!string.IsNullOrWhiteSpace(phoneE164))
        {
            var samePhone = await _credentialsRepository
                .Find(c => c.NormalizedPhoneNumber == phoneE164)
                .AnyAsync();
            if (samePhone) return new AuthResult(false, null, null, "Phone already in use");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = string.IsNullOrWhiteSpace(username) ? GenerateUsernameFromEmailOrRandom(email) : username!,
            Email = string.IsNullOrWhiteSpace(email) ? string.Empty : email!
        };

        var passwordHash = _passwordHasher.HashPassword(user, dto.Password);

        var credentials = new UserCredentials
        {
            UserId = user.Id,
            User = user,
            PasswordHash = passwordHash,
            NormalizedPhoneNumber = phoneE164,
            EmailConfirmed = false,
            PhoneConfirmed = false
        };

        _userRepository.Create(user);
        _credentialsRepository.Create(credentials);

        await _userRepository.SaveChangesAsync();
        await _credentialsRepository.SaveChangesAsync();

        var (token, expires) = CreateJwtForUser(user);

        return new AuthResult(true, token, expires, null, user.Id, user.Email);
    }

    public async Task<AuthResult> LoginAsync(LoginDto dto)
    {
        if (dto == null) return new AuthResult(false, null, null, "Payload is null");

        var idType = dto.IdentifierType;
        var identifier = dto.Identifier?.Trim() ?? string.Empty;
        if (idType == IdentifierType.Auto)
            idType = InferIdentifierType(identifier);

        User? user = null;
        UserCredentials? credentials = null;

        if (idType == IdentifierType.Email)
        {
            var email = identifier.ToLowerInvariant();
            user = await _userRepository.Find(u => u.Email.ToLower() == email).FirstOrDefaultAsync();
            if (user != null)
                credentials = await _credentialsRepository.Find(c => c.UserId == user.Id).FirstOrDefaultAsync();
        }
        else if (idType == IdentifierType.UserName)
        {
            var username = identifier;
            user = await _userRepository.Find(u => u.UserName == username).FirstOrDefaultAsync();
            if (user != null)
                credentials = await _credentialsRepository.Find(c => c.UserId == user.Id).FirstOrDefaultAsync();
        }
        else if (idType == IdentifierType.PhoneNumber)
        {
            if (!TryNormalizePhoneToE164(identifier, out var e164))
                return new AuthResult(false, null, null, "Invalid phone");
            credentials = await _credentialsRepository.Find(c => c.NormalizedPhoneNumber == e164).FirstOrDefaultAsync();
            if (credentials != null)
                user = await _userRepository.Find(u => u.Id == credentials.UserId).FirstOrDefaultAsync();
        }

        if (user == null || credentials == null)
            return new AuthResult(false, null, null, "Invalid credentials");

        var verify = _passwordHasher.VerifyHashedPassword(user, credentials.PasswordHash, dto.Password);
        if (verify == PasswordVerificationResult.Failed)
            return new AuthResult(false, null, null, "Invalid credentials");

        var (token, expires) = CreateJwtForUser(user);
        return new AuthResult(true, token, expires, null, user.Id, user.Email);
    }

    public async Task<AuthResult> ExternalLoginAsync(ExternalAuthDto dto)
    {
        if (dto == null) return new AuthResult(false, null, null, "Payload is null");

        string? email = dto.Email?.Trim().ToLowerInvariant();
        User? user = null;

        if (!string.IsNullOrWhiteSpace(email))
        {
            user = await _userRepository.Find(u => u.Email.ToLower() == email).FirstOrDefaultAsync();
        }

        if (user == null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = email ?? string.Empty,
                UserName = string.IsNullOrWhiteSpace(dto.Name) ? GenerateUsernameFromEmailOrRandom(email) : dto.Name!
            };

            _userRepository.Create(user);
            await _userRepository.SaveChangesAsync();

            var credentials = new UserCredentials
            {
                UserId = user.Id,
                User = user,
                PasswordHash = string.Empty,
                EmailConfirmed = !string.IsNullOrWhiteSpace(email)
            };
            _credentialsRepository.Create(credentials);
            await _credentialsRepository.SaveChangesAsync();
        }

        var (token, expires) = CreateJwtForUser(user);
        return new AuthResult(true, token, expires, null, user.Id, user.Email);
    }

    private (string token, DateTimeOffset expires) CreateJwtForUser(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
        };

        var expires = DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.ExpiresMinutes);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires.UtcDateTime,
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return (tokenString, expires);
    }

    private static string GenerateUsernameFromEmailOrRandom(string? email)
    {
        if (!string.IsNullOrWhiteSpace(email))
        {
            var beforeAt = email.Split('@')[0];
            return beforeAt + "_" + Guid.NewGuid().ToString("N").Substring(0, 6);
        }

        return "user_" + Guid.NewGuid().ToString("N").Substring(0, 8);
    }

    private static readonly Regex EmailRegex =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static IdentifierType InferIdentifierType(string identifier)
    {
        var trimmed = identifier.Trim();
        if (EmailRegex.IsMatch(trimmed)) return IdentifierType.Email;
        if (TryNormalizePhoneToE164(trimmed, out _)) return IdentifierType.PhoneNumber;
        return IdentifierType.UserName;
    }

    private static bool TryNormalizePhoneToE164(string input, out string e164)
    {
        e164 = string.Empty;
        if (string.IsNullOrWhiteSpace(input)) return false;
        var s = input.Trim();
        if (s.StartsWith('+'))
        {
            var digits = new string(s.Skip(1).Where(char.IsDigit).ToArray());
            if (digits.Length >= 8 && digits.Length <= 15)
            {
                e164 = "+" + digits;
                return true;
            }

            return false;
        }

        var allDigits = new string(s.Where(char.IsDigit).ToArray());
        if (allDigits.Length < 8 || allDigits.Length > 15) return false;
        e164 = "+" + allDigits;
        return true;
    }
}