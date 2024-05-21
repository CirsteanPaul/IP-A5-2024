using Carter;
using Dapper;
using IP.Project.Contracts;
using IP.Project.Entities;
using IP.Project.Shared;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using IP.Project.Database;
using Mapster;

namespace IP.Project.Features.Vpn
{
    public class GetVpn
    {
        public class Query : IRequest<Result<VpnResponse>>
        {
            public Guid Id { get; set; }
        }

        public class Validator : AbstractValidator<Query>
        {
            public Validator()
            {
                RuleFor(x => x.Id).NotEmpty().WithMessage("The VPN ID must not be empty.");
            }
        }

        public sealed class Handler : IRequestHandler<Query, Result<VpnResponse>>
        {
            private readonly ISqlConnectionFactory factory;

            public Handler(ISqlConnectionFactory factory)
            {
                factory = factory;
            }

            public async Task<Result<VpnResponse>> Handle(Query request, CancellationToken cancellationToken)
            {
                using (var connection = factory.CreateConnection())
                {
                    var query = "SELECT * FROM dbo.Vpns WHERE Id = @Id";
                    var vpnAccount = await connection.QuerySingleOrDefaultAsync<VpnAccount>(query, new { request.Id });

                    if (vpnAccount == null)
                    {
                        return Result.Failure<VpnResponse>(new Error("GetVpn.NotFound", "VPN account not found"));
                    }

                    var response = vpnAccount.Adapt<VpnResponse>();

                    return Result.Success(response);
                }
            }
        }
    }

    public class GetVpnEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet($"{Global.version}vpns/{{id:guid}}", async ([FromRoute] Guid id, ISender sender, CancellationToken cancellationToken) =>
            {
                var query = new GetVpn.Query { Id = id };
                var result = await sender.Send(query, cancellationToken);
                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.NotFound(new { Message = result.Error.Message });
            })
            .WithTags("Vpn")
            .Produces<VpnResponse>(200)
            .Produces<Error>(404)
            .WithDescription("Retrieves details of a specific VPN account.")
            .WithOpenApi();
        }
    }
}
