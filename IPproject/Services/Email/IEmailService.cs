namespace IP.Project.Services.Email;

public interface IEmailService
{
    Task<bool> SendEmailAsync(Mail email);
}