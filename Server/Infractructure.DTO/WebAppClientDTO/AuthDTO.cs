namespace Infractructure.DTO.WebAppClientDTO;

public record RegisterDto(
    string? Email,
    string? UserName,
    string? PhoneNumber,
    string Name,
    string Password,
    string ConfirmPassword
);

public enum IdentifierType
{
    Auto,
    Email,
    UserName,
    PhoneNumber
}

public record LoginDto(
    string Identifier,
    string Password,
    IdentifierType IdentifierType = IdentifierType.Auto,
    bool RememberMe = false
);

public record ExternalAuthDto(
    string Provider,
    string ProviderKey,
    string? Email,
    string? Name,
    string? PictureUrl,
    string? IdToken
);

public record ExternalLoginCallbackDto(
    string Provider,
    string IdToken,
    string AccessToken
);

public record AuthResult(
    bool Success,
    string? Token,
    DateTimeOffset? Expires,
    string? Error,
    Guid? UserId = null,
    string? Email = null
);