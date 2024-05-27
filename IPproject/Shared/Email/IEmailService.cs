namespace IP.Project.Shared.Email;

public interface IEmailService
{
    Task<bool> SendEmailAsync(Mail email);
}