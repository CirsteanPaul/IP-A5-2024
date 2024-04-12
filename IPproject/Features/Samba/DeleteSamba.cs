using Carter;
using IP.Project.Database;
using IP.Project.Entities;
using IP.Project.Shared;
using MediatR;

namespace IP.Project.Features.Samba;

public static class DeleteSamba
{
    public record Command : IRequest<Result>
    {
        public Guid Id { get; set; }
    }
    
    public class Handler: IRequestHandler<Command, Result>
    {
        private readonly ApplicationDBContext dbContext;

        public Handler(ApplicationDBContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var accountToBeDeleted = await dbContext.FindAsync<SambaAccount>(request.Id);
            
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
        app.MapDelete("api/samba/{id}", async (Guid id, ISender sender) =>
        {
            var command = new DeleteSamba.Command() { Id = id };
            
            var result = await sender.Send(command);
            
            if (result.IsFailure)
            {
                return Results.BadRequest(result.Error);
            }

            return Results.NoContent();
        });
    }
}