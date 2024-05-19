using Carter;
using IP.Project.Contracts;
using IP.Project.Shared;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using IP.Project.Entities;
using System.Data;

namespace IP.Project.Features.Samba
{
    public class GetSamba
    {
        public class Query : IRequest<Result<SambaResponse>>
        {
            public Guid Id { get; set; }
        }

        public sealed class Handler : IRequestHandler<Query, Result<SambaResponse>>
        {
            private readonly IConfiguration _configuration;
            public IDbConnection Connection { get; set; }

            public Handler(IConfiguration configuration)
            {
                _configuration = configuration;
                Connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            }

            public async Task<Result<SambaResponse>> Handle(Query request, CancellationToken cancellationToken)
            {
                var query = "SELECT * FROM SambaAccounts WHERE Id = @Id";
                var samba = await Connection.QuerySingleOrDefaultAsync<SambaAccount>(query, new { Id = request.Id });

                if (samba == null)
                {
                    return Result.Failure<SambaResponse>(new Error("GetSamba.Null", "Samba not found"));
                }

                var sambaResponse = samba.Adapt<SambaResponse>();

                return sambaResponse;
            }
        }
    }

    public class GetSambaEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet($"{Global.version}sambas/{{id:guid}}", async ([FromRoute] Guid id, ISender sender) =>
            {
                var query = new GetSamba.Query
                {
                    Id = id
                };
                var result = await sender.Send(query);
                if (result.IsFailure)
                {
                    return Results.NotFound(result.Error);
                }
                return Results.Ok(result.Value);
            }).WithTags("Samba")
            .WithDescription("Endpoint for retrieving details of a specific Samba account.")
            .Produces<SambaResponse>(StatusCodes.Status200OK)
            .Produces<Error>(StatusCodes.Status404NotFound)
            .WithOpenApi();
        }
    }
}
