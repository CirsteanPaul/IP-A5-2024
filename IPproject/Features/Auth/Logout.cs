using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using IP.Project.Services;
using IP.Project.Shared;

namespace IP.Project.Features.Auth
{
    public class Logout
    {
        public record Command : IRequest<Result>;

        
        public class Handler : IRequestHandler<Command, Result>
        {
            private readonly AuthService _authService;

            public Handler(AuthService authService)
            {
                _authService = authService ?? throw new ArgumentNullException(nameof(authService), "AuthService cannot be null");
            }

            public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
            {
                
                var (status, message) = await _authService.Logout();
                if (status == 1)
                {
                    return Result.Success(); 
                }
                else
                {
                    return Result.Failure(new Error("LogoutFailed", message));  
                }
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
