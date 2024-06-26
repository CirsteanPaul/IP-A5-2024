﻿using Carter;
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
    public class GetAllVpns
    {
        public class Query : IRequest<Result<List<VpnResponse>>>;

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
            app.MapGet($"{Global.Version}vpns", [Authorize] async (ISender sender) =>
            {
                var query = new GetAllVpns.Query();
                var result = await sender.Send(query);

                return Results.Ok(result.Value);
            })
            .WithTags("Vpn");
        }
    }
}
