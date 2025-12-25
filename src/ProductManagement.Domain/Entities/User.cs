namespace ProductManagement.Domain.Entities;

/// <summary>
/// User entity for authentication and authorization
/// Best Practice: Separate authentication from business entities
/// </summary>
public class User : BaseEntity<Guid>
{
    public string Email { get; private set; } = string.Empty;
    public string UserName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public bool IsEmailConfirmed { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiryTime { get; private set; }

    // Navigation property
    private readonly List<UserRole> _userRoles = new();
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    // Computed properties
    public string FullName => $"{FirstName} {LastName}".Trim();

    public static User Create(
        string email,
        string userName,
        string passwordHash,
        string firstName,
        string lastName,
        string phoneNumber)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant(),
            UserName = userName.ToLowerInvariant(),
            PasswordHash = passwordHash,
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phoneNumber,
            IsEmailConfirmed = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System"
        };

        user.Validate();
        return user;
    }

    public void UpdateProfile(string firstName, string lastName, string phoneNumber, string updatedBy)
    {
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;

        Validate();
    }

    public void ChangePassword(string newPasswordHash, string updatedBy)
    {
        PasswordHash = newPasswordHash;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    public void ConfirmEmail()
    {
        IsEmailConfirmed = true;
    }

    public void Activate(string updatedBy)
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    public void Deactivate(string updatedBy)
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    public void SetRefreshToken(string refreshToken, DateTime expiryTime)
    {
        RefreshToken = refreshToken;
        RefreshTokenExpiryTime = expiryTime;
    }

    public void RevokeRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiryTime = null;
    }

    public void AddRole(Role role)
    {
        if (_userRoles.Any(ur => ur.RoleId == role.Id))
            throw new InvalidOperationException($"User already has role '{role.Name}'");

        var userRole = UserRole.Create(Id, role.Id);
        _userRoles.Add(userRole);
    }

    public void RemoveRole(Guid roleId)
    {
        var userRole = _userRoles.FirstOrDefault(ur => ur.RoleId == roleId);
        if (userRole != null)
        {
            _userRoles.Remove(userRole);
        }
    }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(Email))
            throw new InvalidOperationException("Email is required");

        if (!IsValidEmail(Email))
            throw new InvalidOperationException("Invalid email format");

        if (string.IsNullOrWhiteSpace(UserName))
            throw new InvalidOperationException("Username is required");

        if (string.IsNullOrWhiteSpace(FirstName))
            throw new InvalidOperationException("First name is required");

        if (string.IsNullOrWhiteSpace(LastName))
            throw new InvalidOperationException("Last name is required");
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}