using System.Text.RegularExpressions;
using Infractructure.DTO.WebAppClientDTO;
using Microsoft.AspNetCore.Mvc;
using PhotoCloud.Interfaces;

namespace PhotoCloud.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthentificationController : BaseController
{
    private readonly IAuthService _authService;

    public AuthentificationController(
        ICurrentUserService currentUserService,
        IAuthService authService) : base(currentUserService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (dto is null) return BadRequest("Payload is required.");

        var idType = dto.IdentifierType == IdentifierType.Auto
            ? InferIdentifierType(dto.Identifier)
            : dto.IdentifierType;

        var normalizedIdentifier = dto.Identifier.Trim();
        if (idType == IdentifierType.Email) normalizedIdentifier = normalizedIdentifier.ToLowerInvariant();
        if (idType == IdentifierType.PhoneNumber)
        {
            if (!TryNormalizePhoneToE164(normalizedIdentifier, out var e164))
                return BadRequest("Invalid phone number format.");
            normalizedIdentifier = e164;
        }

        var normalizedDto = dto with { Identifier = normalizedIdentifier, IdentifierType = idType };

        var result = await _authService.LoginAsync(normalizedDto);
        if (!result.Success)
            return Unauthorized(new { error = result.Error });

        return Ok(new { token = result.Token, expires = result.Expires, user = new { id = result.UserId, email = result.Email } });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (dto is null) return BadRequest("Payload is required.");

        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password != dto.ConfirmPassword)
            return BadRequest("Passwords are missing or do not match.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Name is required.");

        var email = dto.Email?.Trim().ToLowerInvariant();
        if (email != null && !EmailRegex.IsMatch(email))
            return BadRequest("Invalid email format.");

        string? phoneNormalized = null;
        if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
        {
            if (!TryNormalizePhoneToE164(dto.PhoneNumber, out var e164Phone))
                return BadRequest("Invalid phone number format.");
            phoneNormalized = e164Phone;
        }

        var normalizedDto = dto with { Email = email, PhoneNumber = phoneNormalized };

        var result = await _authService.RegisterAsync(normalizedDto);
        if (!result.Success)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(null, new { token = result.Token, expires = result.Expires, user = new { id = result.UserId, email = result.Email } });
    }
    
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private IdentifierType InferIdentifierType(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier)) return IdentifierType.UserName;

        var trimmed = identifier.Trim();
        if (EmailRegex.IsMatch(trimmed)) return IdentifierType.Email;
        if (TryNormalizePhoneToE164(trimmed, out _)) return IdentifierType.PhoneNumber;
        return IdentifierType.UserName;
    }

    private bool TryNormalizePhoneToE164(string input, out string e164)
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
