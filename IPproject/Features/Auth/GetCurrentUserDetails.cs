using Carter;
using MediatR;
using System.Threading.Tasks;
using IP.Project.Shared;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Security.Claims;
using IP.Project.Contracts;
using Microsoft.Identity.Web;


namespace IP.Project.Features.Auth
{
    public class GetCurrentUserDetails
    {
        public class Query : IRequest<Result<CurrentUser>>
        {
        }

        public class Handler : IRequestHandler<Query, Result<CurrentUser>>
        {
            private readonly IHttpContextAccessor httpContextAccessor;
     

            public Handler(IHttpContextAccessor httpContextAccessor)
            {
                this.httpContextAccessor = httpContextAccessor;
            }
            public string UserId => httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)!;
            public ClaimsPrincipal GetCurrentClaimsPrincipal()
            {
                if (httpContextAccessor.HttpContext != null && httpContextAccessor.HttpContext.User != null)
                {
                    return httpContextAccessor.HttpContext.User;
                }

                return null!;
            }
            public string GetCurrentUserId()
            {
                return GetCurrentClaimsPrincipal()?.GetObjectId()!;
            }
            public Task<Result<CurrentUser>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    var user = GetCurrentClaimsPrincipal();
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
                string apiVersion = Global.version;
                app.MapGet($"{apiVersion}auth/getcurrentdetails", async (ISender sender) =>
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
