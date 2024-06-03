using Carter;
using Dapper;
using IP.Project.Contracts.Vpn;
using IP.Project.Database;
using IP.Project.Entities;
using IP.Project.Shared;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace IP.Project.Features.Vpn
{
    public class GetVpn
    {
        public class Query : IRequest<Result<VpnResponse>>
        {
            public Guid Id { get; set; }
        }

        public sealed class Handler : IRequestHandler<Query, Result<VpnResponse>>
        {
            private readonly ISqlConnectionFactory factory;

            public Handler(ISqlConnectionFactory factory)
            {
                this.factory = factory;
            }

            public async Task<Result<VpnResponse>> Handle(Query request, CancellationToken cancellationToken)
            {
                using var connection = factory.CreateConnection();
                var vpn = await connection.QueryFirstOrDefaultAsync<VpnAccount>("SELECT * FROM Vpns WHERE Id = @Id",
                    new { request.Id });

                if (vpn == null)
                {
                    return Result.Failure<VpnResponse>(
                        new Error("GetVpn.Null", "Vpn not found"));
                }

                var vpnResponse = vpn.Adapt<VpnResponse>();

                return vpnResponse;
            }
        }
    }

    public class GetVpnEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet($"{Global.version}vpns/{{id}}", [Authorize] async (Guid id, ISender sender) =>
            {
                var query = new GetVpn.Query
                {
                    Id = id
                };
                var result = await sender.Send(query);
                return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound(result.Error);
            })
            .WithTags("Vpn");
        }
    }
}
