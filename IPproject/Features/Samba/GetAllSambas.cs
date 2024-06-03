using Carter;
using Dapper;
using IP.Project.Contracts.Samba;
using IP.Project.Database;
using IP.Project.Entities;
using IP.Project.Shared;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace IP.Project.Features.Samba
{
    public class GetAllSambas
    {
        public class Query : IRequest<Result<List<SambaResponse>>>;

        public class Handler : IRequestHandler<Query, Result<List<SambaResponse>>>
        {
            private readonly ISqlConnectionFactory factory;

            public Handler(ISqlConnectionFactory factory, ApplicationDBContext dbContext)
            {
                this.factory = factory;
            }

            public async Task<Result<List<SambaResponse>>> Handle(Query request, CancellationToken cancellationToken)
            {
                using (var connection = factory.CreateConnection())
                {
                    var query = "SELECT * FROM SambaAccounts";
                    var sambas = await connection.QueryAsync<SambaAccount>(query);

                    if (!sambas.Any())
                    {
                        return Result.Success(new List<SambaResponse>());
                    }

                    var sambaResponse = sambas.Adapt<List<SambaResponse>>();

                    return Result.Success(sambaResponse);
                }
            }
        }
    }

    public class GetAllSambasEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet($"{Global.Version}sambas", [Authorize] async (ISender sender) =>
                {
                    var query = new GetAllSambas.Query();
                    var result = await sender.Send(query);

                    return Results.Ok(result.Value);
                }).WithTags("Samba")
                .WithDescription("Endpoint for retrieving details of all Samba accounts.")
                .Produces<List<SambaResponse>>()
                .Produces<Error>(StatusCodes.Status500InternalServerError);
        }
    }
}