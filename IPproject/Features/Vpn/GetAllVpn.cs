using Carter;
using IP.Project.Contracts;
using IP.Project.Database;
using IP.Project.Shared;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;


namespace IP.Project.Features.Vpn
{
    public class GetAllVpns
    {
        public class Query : IRequest<Result<List<VpnResponse>>>
        {
        }

        internal sealed class Handler : IRequestHandler<Query, Result<List<VpnResponse>>>
        {
            private readonly ApplicationDBContext context;

            public Handler(ApplicationDBContext context)
            {
                this.context = context;
            }

            public async Task<Result<List<VpnResponse>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var vpns = await context.Vpns.ToListAsync(cancellationToken);

                if (vpns == null || vpns.Count == 0)
                {
                    return Result.Failure<List<VpnResponse>>(
                        new Error("GetAllVpns.Empty", "No VPNs found"));
                }

                var vpnResponses = vpns.Adapt<List<VpnResponse>>();

                return Result.Success(vpnResponses);
            }
        }
    }

    public class GetAllVpnsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/vpns", async (ISender sender) =>
            {
                var query = new GetAllVpns.Query();
                var result = await sender.Send(query);

                return result.IsSuccess ?
                    Results.Ok(result.Value) :
                    Results.NotFound(result.Error);
            }).WithTags("vpn");
        }
    }
}
