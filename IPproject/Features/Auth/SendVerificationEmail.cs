using Carter;
using FluentValidation;
using IP.Project.Contracts;
using IP.Project.Entities;
using IP.Project.Shared;
using MailKit.Net.Smtp;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MimeKit;

namespace IP.Project.Features.Auth;

public static class SendVerificationEmail
{
    public record Command : IRequest<Result>
    {
        public string Email { get; init; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
        }
    }

    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Validator _validator;

        public Handler(UserManager<ApplicationUser> userManager, Validator validator)
        {
            _userManager = userManager;
            _validator = validator;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage);
                return Result.Failure(new Error("ValidationFailed", string.Join("\n", errors)));
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Result.Failure(new Error("UserNotFound", "User not found."));
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var verificationLink =
                $"http://localhost:3000/verify-email?userId={Uri.EscapeDataString(user.Id)}&token={Uri.EscapeDataString(token)}";
            
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse("App <app@example.com>"));
            email.To.Add(MailboxAddress.Parse(user.Email));
            email.Subject = "Email Verification";
            email.Body = new TextPart("plain")
            {
                Text =
                    $"Hello {user.UserName},\n\nPlease click the following link to verify your email:\n{verificationLink}\n\nIf you did not request email verification, please ignore this email."
            };

            try
            {
                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync("smtp.example.com", 587, false, cancellationToken);
                    await client.AuthenticateAsync("smtp_username", "smtp_password", cancellationToken);
                    await client.SendAsync(email, cancellationToken);
                    await client.DisconnectAsync(true, cancellationToken);
                }
            }
            catch (Exception e)
            {
                return Result.Failure(new Error("EmailProviderError", e.Message));
            }

            return Result.Success();
        }
    }
}

public class SendVerificationEmailEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost($"{Global.version}auth/send-verification-email", async ([FromBody] SendVerificationEmailRequest request, ISender sender) =>
            {
                var command = new SendVerificationEmail.Command { Email = request.Email };
                var result = await sender.Send(command);

                if (!result.IsSuccess)
                {
                    return Results.BadRequest(result.Error);
                }

                return Results.Ok();
            })
            .WithTags("Auth")
            .WithDescription("Endpoint for sending email verification links.")
            .Produces(StatusCodes.Status200OK)
            .Produces<Error>(StatusCodes.Status400BadRequest)
            .WithOpenApi();
    }
}