namespace WebApi.Models.Dtos;

public record User
{
    public string Id { get; init; }
    public string Email { get; init; }
    public bool EmailVerified { get; init; }

    public User(string id, string email, bool emailVerified)
    {
        Id = id;
        Email = email;
        EmailVerified = emailVerified;
    }
};