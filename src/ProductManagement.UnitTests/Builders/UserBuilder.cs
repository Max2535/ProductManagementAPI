using ProductManagement.Domain.Entities;

namespace ProductManagement.UnitTests.Builders;

public class UserBuilder
{
    private string _email = "test@example.com";
    private string _userName = "testuser";
    private string _passwordHash = "hashedpassword";
    private string _firstName = "Test";
    private string _lastName = "User";
    private string _phoneNumber = "+66812345678";

    public UserBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public UserBuilder WithUsername(string username)
    {
        _userName = username;
        return this;
    }

    public UserBuilder WithPassword(string passwordHash)
    {
        _passwordHash = passwordHash;
        return this;
    }

    public User Build()
    {
        return User.Create(
            _email,
            _userName,
            _passwordHash,
            _firstName,
            _lastName,
            _phoneNumber);
    }
}