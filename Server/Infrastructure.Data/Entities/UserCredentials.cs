namespace Infrastructure.Data.Entities;

public class UserCredentials
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }         
    public User User { get; set; } = null!;

    public required string PasswordHash { get; set; }
    public string? PasswordSalt { get; set; } 

    public string? NormalizedPhoneNumber { get; set; } // E.164
    public bool EmailConfirmed { get; set; }
    public bool PhoneConfirmed { get; set; }
}