using Carter;
using IP.Project.Contracts;
using IP.Project.Database;
using IP.Project.Shared;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IP.Project.Features.Vpn
{
    public class GetAllVpns
    {
        public class Query : IRequest<Result<List<VpnResponse>>>
        {
        }

        public class Handler : IRequestHandler<Query, Result<List<VpnResponse>>>
        {
            private readonly IConfiguration _configuration;

            public Handler(IConfiguration configuration)
            {
                _configuration = configuration;
            }

            public async Task<Result<List<VpnResponse>>> Handle(Query request, CancellationToken cancellationToken)
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("Database")))
                {
                    await connection.OpenAsync(cancellationToken);

                    var query = "SELECT * FROM Vpns";
                    var vpns = await connection.QueryAsync<IP.Project.Entities.VpnAccount>(query);


                    if (vpns == null || vpns.AsList().Count == 0)
                    {
                        return Result.Success(new List<VpnResponse>());
                    }

                    var vpnResponses = vpns.Adapt<List<VpnResponse>>();
                    return Result.Success(vpnResponses);
                }
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
