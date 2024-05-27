using Carter;
using MediatR;
using Microsoft.AspNetCore.Identity;
using IP.Project.Shared;
using IP.Project.Entities;

namespace IP.Project.Features.Auth
{
    public static class Logout
    {
        public record Command : IRequest<Result>
        {
        }

        public class Handler : IRequestHandler<Command, Result>
        {
            private readonly SignInManager<ApplicationUser> _signInManager;

            public Handler(SignInManager<ApplicationUser> signInManager)
            {
                _signInManager = signInManager;
            }

            public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {
                    await _signInManager.SignOutAsync();
                }
                catch (Exception e)
                {
                    return Result.Failure(new Error("InternalServerError", e.Message));
                }

                return Result.Success();
            }
        }
    }

    public class LogoutEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            _ = app.MapPost($"{Global.version}auth/logout", async (ISender sender) =>
            {
                var command = new Logout.Command();
                var result = await sender.Send(command);

                if (!result.IsSuccess)
                {
                    return Results.BadRequest(result.Error);
                }

                return Results.Ok();
            })
            .WithTags("Auth")
            .WithDescription("Endpoint for logging out the current user.")
            .Produces(StatusCodes.Status200OK)
            .Produces<Error>(StatusCodes.Status400BadRequest)
            .WithOpenApi();
        }
    }
}
