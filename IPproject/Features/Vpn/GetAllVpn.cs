using Carter;
using Dapper;
using IP.Project.Contracts;
using IP.Project.Database;
using IP.Project.Entities;
using IP.Project.Shared;
using Mapster;
using MediatR;

namespace IP.Project.Features.Vpn
{
    public class GetAllVpns
    {
        public class Query : IRequest<Result<List<VpnResponse>>>
        {
        }

        public class Handler : IRequestHandler<Query, Result<List<VpnResponse>>>
        {
            private readonly ISqlConnectionFactory factory;

            public Handler(ISqlConnectionFactory factory)
            {
                this.factory = factory;
            }

            public async Task<Result<List<VpnResponse>>> Handle(Query request, CancellationToken cancellationToken)
            {
                using var connection = factory.CreateConnection();
                var vpns = await connection.QueryAsync<VpnAccount>("SELECT * FROM Vpns");

                if (!vpns.Any())
                {
                    return Result.Success(new List<VpnResponse>());
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
            app.MapGet("api/v1/vpns", async (ISender sender) =>
            {
                var query = new GetAllVpns.Query();
                var result = await sender.Send(query);

                return result.IsSuccess ?
                    Results.Ok(result.Value) :
                    Results.NotFound(result.Error);
            })
            .WithTags("Vpn");
        }
    }
}
