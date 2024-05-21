using Carter;
using FluentValidation;
using IP.Project.Database;
using IP.Project.Features.Vpn;
using IP.Project.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IP.Project.Features.Vpn
{
    public static class DeleteVpn
    {
        public record Command(Guid Id) : IRequest<Result>;

        public class Handler : IRequestHandler<Command, Result>
        {
            private readonly ApplicationDBContext dbContext;

            public Handler(ApplicationDBContext dbContext)
            {
                this.dbContext = dbContext;
            }

            public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
            {
                var vpnToBeDeleted = await dbContext.Vpns.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

                if (vpnToBeDeleted is null)
                {
                    return Result.Failure<Guid>(new Error("DeleteVpn.Null", $"Vpn with id {request.Id} not found"));
                }
                dbContext.Vpns.Remove(vpnToBeDeleted);
                await dbContext.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
        }
    }
}

public class DeleteVpnEndpoint :ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        _ = app.MapDelete("api/v1/vpns/{id}", async (Guid id, ISender sender) =>
        {
            var command = new DeleteVpn.Command(id);
            var result = await sender.Send(command);
            if (result.IsFailure)
            {
                if (result.Error.Code == "DeleteVpn.Null")
                {
                    return Results.NotFound(result.Error);
                }
                return Results.BadRequest(result.Error);
            }
            return Results.NoContent();
        })
        .WithTags("Vpn")
        .WithDescription("Endpoint for deleting a specific Vpn. " +
                         "If the request is successful, it will return status code 204 (No content).")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<Error>(StatusCodes.Status404NotFound)
        .WithOpenApi();
    }   
}

