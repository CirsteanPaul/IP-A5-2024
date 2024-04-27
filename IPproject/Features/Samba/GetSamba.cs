using Carter;
using IP.Project.Contracts;
using IP.Project.Database;
using IP.Project.Features.Samba;
using IP.Project.Shared;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            private readonly ApplicationDBContext context;

            public Handler(ApplicationDBContext context)
            {
                this.context = context;
            }

            public async Task<Result<SambaResponse>> Handle(Query request, CancellationToken cancellationToken)
            {
                var samba = await context.SambaAccounts.AsNoTracking().FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

                if (samba == null)
                {
                    return Result.Failure<SambaResponse>(
                        new Error("GetSamba.Null", "Samba not found"));
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
        .WithDescription("Endpoint for retrieving details of a specific Samba account. " +
                         "If the request is successful, returns details of the specified Samba account. ")
        .Produces(StatusCodes.Status200OK)
        .Produces<Error>(StatusCodes.Status404NotFound)
        .WithOpenApi();
    }
}