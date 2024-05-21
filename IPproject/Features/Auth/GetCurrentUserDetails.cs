
using Carter;
using IP.Project.Services;
using MediatR;
using System.Threading.Tasks;
using IP.Project.Shared;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using IP.Project.Models;

namespace IP.Project.Features.Auth
{
    public class GetCurrentUserDetails
    {
        public record Query : IRequest<Result<CurrentUser>>;

        public class Handler : IRequestHandler<Query, Result<CurrentUser>>
        {
            private readonly CurrentUserService _currentUserService;

            public Handler(CurrentUserService currentUserService)
            {
                _currentUserService = currentUserService;
            }

            public Task<Result<CurrentUser>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    var user = _currentUserService.GetCurrentClaimsPrincipal();
                    if (user == null)
                        return Task.FromResult(Result.Failure<CurrentUser>(new Error("User.NotFound", "No current user found")));

                    var userDetails = new CurrentUser
                    {
                        UserName = user.Identity.Name,
                        IsAuthenticated = user.Identity.IsAuthenticated,
                        Claims = user.Claims.ToDictionary(c => c.Type, c => c.Value)
                    };

                    return Task.FromResult(Result.Success(userDetails));
                }
                catch (Exception ex)
                {
                    return Task.FromResult(Result.Failure<CurrentUser>(new Error("Exception", ex.Message)));
                }
            }
        }

        public class GetCurrentUserDetailsEndpoint : ICarterModule
        {
            public void AddRoutes(IEndpointRouteBuilder app)
            {
                app.MapGet("api/v1/auth/current-user", async (ISender sender) =>
                {
                    var result = await sender.Send(new GetCurrentUserDetails.Query());
                    if (result.IsSuccess)
                        return Results.Ok(result.Value);
                    return Results.BadRequest(result.Error);
                })
                .WithTags("Auth")
                .WithDescription("Endpoint for retrieving the current user's details.");
            }
        }
    }
}
