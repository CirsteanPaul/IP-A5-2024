using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using IP.Project.Models;
using IP.Project.Shared;
using Microsoft.AspNetCore.Identity;

namespace IP.Project.Features.Auth
{
    public class Logout
    {
        public record Command : IRequest<Result>
        {
        }

        
        public class Handler : IRequestHandler<Command, Result>
        {
            private readonly SignInManager<ApplicationUser> signInManager;

            public Handler(SignInManager<ApplicationUser> signInManager)
            {
                this.signInManager = signInManager;
            }

            public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
            {
                
                try
                {
                    await signInManager.SignOutAsync();
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
            app.MapPost("api/v1/auth/logout", async (ISender sender) =>
            {
                var result = await sender.Send(new Logout.Command());
                if (result.IsSuccess)
                    return Results.Ok();
                else
                    return Results.BadRequest(result.Error);
            })
            .WithTags("Auth")
            .WithDescription("Endpoint for logging out the current user.");
        }
    }
}
