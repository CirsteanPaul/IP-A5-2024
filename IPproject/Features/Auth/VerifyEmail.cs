using Carter;
using FluentValidation;
using IP.Project.Contracts;
using IP.Project.Entities;
using IP.Project.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IP.Project.Features.Auth
{
    public static class VerifyEmail
    {
        public record Command : IRequest<Result>
        {
            public string UserId { get; init; } = string.Empty;
            public string Token { get; init; } = string.Empty;
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.UserId).NotEmpty();
                RuleFor(x => x.Token).NotEmpty();
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

                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                {
                    return Result.Failure(new Error("UserNotFound", "User not found."));
                }

                var result = await _userManager.ConfirmEmailAsync(user, request.Token);
                if (!result.Succeeded)
                {
                    return Result.Failure(new Error("EmailConfirmationFailed", "Email verification failed."));
                }

                return Result.Success();
            }
        }
    }

    public class VerifyEmailEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost($"{Global.version}auth/verify-email", async ([FromBody] VerifyEmailRequest request, ISender sender) =>
                {
                    var command = new VerifyEmail.Command { UserId = request.UserId, Token = request.Token };
                    var result = await sender.Send(command);

                    if (!result.IsSuccess)
                    {
                        return Results.BadRequest(result.Error);
                    }

                    return Results.Ok();
                })
                .WithTags("Auth")
                .WithDescription("Endpoint for verifying email addresses. It confirms the user's email using a verification token.")
                .Produces(StatusCodes.Status200OK)
                .Produces<Error>(StatusCodes.Status400BadRequest)
                .WithOpenApi();
        }
    }
}
