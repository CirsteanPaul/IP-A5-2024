using FluentValidation;
using IP.Project.Models;
using IP.Project.Contracts;

using IP.Project.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace IP.Project.Features.Auth
{
    public class CreateResetPasswordLink
    {
        public class ForgotPasswordRequest
        {
            public string Email { get; set; } = string.Empty;
        }

        public class ForgotPasswordResponse
        {
            public string ResetLink { get; init; } = string.Empty;
        }

        public record Command(ForgotPasswordRequest Request) : IRequest<Result<ForgotPasswordResponse>>;

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Request.Email).NotEmpty().EmailAddress();
            }
        }

        public class Handler : IRequestHandler<Command, Result<ForgotPasswordResponse>>
        {
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly IConfiguration _configuration;

            public Handler(UserManager<ApplicationUser> userManager, IConfiguration configuration)
            {
                _userManager = userManager;
                _configuration = configuration;
            }

            public async Task<Result<ForgotPasswordResponse>> Handle(Command command, CancellationToken cancellationToken)
            {
                var validationResult = new Validator().Validate(command);
                if (!validationResult.IsValid)
                {
                    return Result.Failure<ForgotPasswordResponse>(new Error("ValidationError", string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))));
                }

                var user = await _userManager.FindByEmailAsync(command.Request.Email);
                if (user == null)
                {
                    return Result.Failure<ForgotPasswordResponse>(new Error("UserNotFound", "No user found with the specified email."));
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetLink = $"{_configuration["FrontendUrl"]}/reset-password?token={token}&email={command.Request.Email}";

                try
                {
                    using (var message = new MailMessage())
                    {
                        message.To.Add(new MailAddress(command.Request.Email));
                        message.Subject = "Password Reset";
                        message.Body = $"Please reset your password using the following link: {resetLink}";

                        using (SmtpClient smtpClient = new SmtpClient())
                        {
                            smtpClient.Send(message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    return Result.Failure<ForgotPasswordResponse>(new Error("EmailSendingFailed", $"Failed to send email: {ex.Message}"));
                }

                return Result.Success(new ForgotPasswordResponse { ResetLink = resetLink });
            }
        }
    }
}
