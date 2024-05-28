using Carter;
using FluentValidation;
using IP.Project.Entities;
using IP.Project.Services.Email;
using IP.Project.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity.Data;

namespace IP.Project.Features.Auth
{
    public static class ForgotPassword
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
            private readonly IEmailService emailService;
            private readonly ILogger<Handler> logger;

            public Handler(UserManager<ApplicationUser> userManager, Validator validator, IEmailService emailService, ILogger<Handler> logger)
            {
                _userManager = userManager;
                _validator = validator;
                this.emailService = emailService;
                this.logger = logger;
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
                    // Don't reveal that the user does not exist or is not confirmed
                    return Result.Success();
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetLink = $"http://localhost:3000/reset-password?token={Uri.EscapeDataString(token)}";
                
                var email = new Mail
                {
                    To = request.Email,
                    Subject = "Reset password",
                    Body = resetLink + " " + token
                };

                try
                {
                    await emailService.SendEmailAsync(email);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Email sending failed");
                    return Result.Failure(new Error("EmailProviderError", e.Message));
                }
                
                return Result.Success();
            }
        }
    }

    public class ForgotPasswordEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost($"{Global.version}auth/forgot-password", async ([FromBody] ForgotPasswordRequest request, ISender sender) =>
                {
                    var command = new ForgotPassword.Command { Email = request.Email };
                    var result = await sender.Send(command);

                    if (!result.IsSuccess)
                    {
                        return Results.BadRequest(result.Error);
                    }
                    
                    return Results.Ok();
                })
                .WithTags("Auth")
                .WithDescription("Endpoint for initiating the forgot password process. It sends a password reset link to the user's email address.")
                .Produces(StatusCodes.Status200OK)
                .Produces<Error>(StatusCodes.Status400BadRequest)
                .WithOpenApi();

        }
    }
}
