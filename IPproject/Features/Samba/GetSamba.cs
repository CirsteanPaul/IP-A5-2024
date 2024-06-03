using Carter;
using IP.Project.Shared; 
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using IP.Project.Contracts.Samba;
using IP.Project.Entities;
using IP.Project.Database;
using Microsoft.AspNetCore.Authorization;

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
            private readonly ISqlConnectionFactory _factory;

            public Handler(ISqlConnectionFactory factory)
            {
                _factory = factory;
            }

            public async Task<Result<SambaResponse>> Handle(Query request, CancellationToken cancellationToken)
            {
                using (var connection = _factory.CreateConnection())
                {
                    var query = "SELECT * FROM SambaAccounts WHERE Id = @Id";
                    var samba = await connection.QuerySingleOrDefaultAsync<SambaAccount>(query, new { request.Id });

                    if (samba == null)
                    {
                        return Result.Failure<SambaResponse>(new Error("GetSamba.Null", "Samba not found"));
                    }

                    var sambaResponse = samba.Adapt<SambaResponse>();
                    
                    return sambaResponse;
                }
            }
        }
    }

    public class GetSambaEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet($"{Global.version}sambas/{{id:guid}}", [Authorize] async ([FromRoute] Guid id, ISender sender) =>
            {
                var query = new GetSamba.Query
                {
                    Id = id
                };
                var result = await sender.Send(query);
                
                return result.IsFailure ? Results.NotFound(result.Error) : Results.Ok(result.Value);
            }).WithTags("Samba")
            .WithDescription("Endpoint for retrieving details of a specific Samba account.")
            .Produces<SambaResponse>()
            .Produces<Error>(StatusCodes.Status404NotFound)
            .WithOpenApi();
        }
    }
}
