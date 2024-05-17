using System.ComponentModel.DataAnnotations;

namespace IP.Project.Contracts;

public class RegisterRequest
{
    public string? Username { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    public string? Password { get; set; }
}