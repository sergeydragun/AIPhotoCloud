using Infractructure.DTO.WebAppClientDTO;

namespace PhotoCloud.Interfaces;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterDto dto);
    Task<AuthResult> LoginAsync(LoginDto dto);
    Task<AuthResult> ExternalLoginAsync(ExternalAuthDto dto);
}