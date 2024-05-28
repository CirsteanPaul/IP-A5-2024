using Carter;
using IP.Project.Entities;
using IP.Project.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using Mapster;

namespace IP.Project.Features.Auth
{
    public static class ResetPassword
    {
        public record Command : IRequest<Result>
        {
            public string Email { get; init; }
            public string NewPassword { get; init; }
            public string ResetCode { get; init; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Email).NotEmpty().EmailAddress();
                RuleFor(x => x.ResetCode).NotEmpty();
            }
        }

        public class Handler : IRequestHandler<Command, Result>
        {
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly CommandValidator _validator;

            public Handler(UserManager<ApplicationUser> userManager, CommandValidator validator)
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
                    // Don't reveal that the user does not exist or is not confirmed
                    return Result.Success();
                }

                var result = await _userManager.ResetPasswordAsync(user, request.ResetCode, request.NewPassword);
                if (result.Succeeded)
                {
                    return Result.Success();
                }
                
                return Result.Failure(new Error("ResetPasswordFailed", string.Join("\n", result.Errors.Select(x => x.Description))));
            }
        }
    }

    public class ResetPasswordEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost($"{Global.version}auth/reset-password",
            async ([FromBody] ResetPasswordRequest request, ISender sender) =>
            {
                var command = request.Adapt<ResetPassword.Command>();
                
                var result = await sender.Send(command);

                if (!result.IsSuccess)
                {
                    return Results.BadRequest(result.Error);
                }

                return Results.Ok();
            })
            .WithTags("Auth")
            .WithDescription("Endpoint for resetting user's password using the reset token.")
            .Produces(StatusCodes.Status200OK)
            .Produces<Error>(StatusCodes.Status400BadRequest)
            .WithOpenApi();
        }
    }
}
