namespace ProductManagement.Application.Interfaces;

/// <summary>
/// Password hashing interface
/// Best Practice: Never store plain text passwords
/// </summary>
public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}