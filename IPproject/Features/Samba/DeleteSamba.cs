using Carter;
using IP.Project.Database;
using IP.Project.Entities;
using IP.Project.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IP.Project.Features.Samba;

public static class DeleteSamba
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
            var accountToBeDeleted = await dbContext.SambaAccounts.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            
            if (accountToBeDeleted is null)
            {
                return Result.Failure(new Error("DeleteSamba.Null", $"Samba account with id {request.Id} not found"));
            }

            dbContext.SambaAccounts.Remove(accountToBeDeleted);

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}

public class DeleteSambaEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/v1/sambas/{id}", async (Guid id, ISender sender) =>
        {
            var command = new DeleteSamba.Command(id);
            var result = await sender.Send(command);

            if (result.IsFailure)
            {
                if (result.Error.Code == "DeleteSamba.Null")
                {
                    return Results.NotFound(result.Error);
                }
                return Results.BadRequest(result.Error);
            }

            return Results.NoContent();
        })
        .WithTags("Samba"); 
    }
}
