using ProductManagement.Application.Interfaces;

namespace ProductManagement.Infrastructure.Services;

/// <summary>
/// Password hashing using BCrypt
/// Best Practice: Use industry-standard hashing algorithms
/// BCrypt automatically handles salting and multiple rounds
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12; // Cost factor for BCrypt (higher = more secure but slower)

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty", nameof(password));

        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty", nameof(password));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty", nameof(passwordHash));

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch
        {
            return false;
        }
    }
}