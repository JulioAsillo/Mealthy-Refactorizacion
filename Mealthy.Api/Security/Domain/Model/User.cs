using Mealthy.Api.Shared.Domain.Model;

namespace Mealthy.Api.Security.Domain.Model;

public class User : Entity
{
    public string Username { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string? Phone { get; private set; }
    public DateOnly? BirthDate { get; private set; }
    public Role Role { get; private set; }

    // Requerido por EF Core
    private User() { }

    public User(string username, string email, string passwordHash,
        string firstName, string lastName, string? phone, DateOnly? birthDate, Role role)
    {
        Username = username.Trim();
        Email = NormalizeEmail(email);
        PasswordHash = passwordHash;
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Phone = phone?.Trim();
        BirthDate = birthDate;
        Role = role;
    }

    public void UpdateProfile(string firstName, string lastName, string? phone, DateOnly? birthDate)
    {
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Phone = phone?.Trim();
        BirthDate = birthDate;
    }

    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
