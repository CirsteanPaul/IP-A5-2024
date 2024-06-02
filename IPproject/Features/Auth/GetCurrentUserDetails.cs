using System.Security.Claims;
using Carter;
using IP.Project.Contracts.Auth;
using IP.Project.Shared;
using MediatR;

namespace IP.Project.Features.Auth
{
    public class GetCurrentUserDetails
    {
        public class Query : IRequest<Result<CurrentUserDetails>>
        {
        }

        public class Handler : IRequestHandler<Query, Result<CurrentUserDetails>>
        {
            private readonly IHttpContextAccessor _httpContextAccessor;

            public Handler(IHttpContextAccessor httpContextAccessor)
            {
                this._httpContextAccessor = httpContextAccessor;
            }

            private ClaimsPrincipal? GetCurrentClaimsPrincipal()
            {
                return _httpContextAccessor.HttpContext?.User;
            }

            private string? GetCurrentUserId()
            {
                return GetCurrentClaimsPrincipal()?.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");
            }

            
            public Task<Result<CurrentUserDetails>> Handle(Query request, CancellationToken cancellationToken)
                {
                    var user = GetCurrentClaimsPrincipal();
                    if (user?.Identity == null  || !user.Identity.IsAuthenticated)
                    {
                        return Task.FromResult(Result.Success(new CurrentUserDetails
                        {
                            IsAuthenticated = false
                        }));
                    }

                    var currentUserDetails = new CurrentUserDetails
                    {
                        IsAuthenticated = true,
                        UserName = GetCurrentUserId()!,
                        Claims = user.Claims.ToDictionary(c => c.Type, c => c.Value)
                    };

                    return Task.FromResult(Result.Success(currentUserDetails));
                }
        }
    }
    
    public class CurrentUserDetailsEndPoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet($"{Global.version}auth/currentUserDetails", async (ISender sender) =>
                {
                    var query = new GetCurrentUserDetails.Query();
                    var result = await sender.Send(query);

                    return Results.Ok(result.Value);
                })
                .WithTags("Auth")
                .WithDescription("Endpoint for retrieving the current user's information. If the request is successful, it will return status code 200 (OK) with the user details.")
                .Produces<CurrentUserDetails>(StatusCodes.Status200OK)
                .Produces<Error>(StatusCodes.Status400BadRequest)
                .WithOpenApi();
        }
    }
}