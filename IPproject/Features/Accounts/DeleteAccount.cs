using Carter;
using IP.Project.Database;
using IP.Project.Entities;
using IP.Project.Shared;
using MediatR;

namespace IP.Project.Features.Accounts;


public class DeleteAccount
{
    public record Command : IRequest<Result>
    {
        public Guid Id { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly ApplicationDBContext dbContext;

        public Handler(ApplicationDBContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var accountToBeDeleted = await dbContext.FindAsync<Account>(request.Id);

            if (accountToBeDeleted is null)
            {
                return Result.Failure(new Error("DeleteAccount.Null", $"Account with id {request.Id} not found"));
            }

            dbContext.Accounts.Remove(accountToBeDeleted);

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}

public class DeleteAccountEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/v1/accounts/{id}", async (Guid id, ISender sender) =>
        {
            var command = new DeleteAccount.Command() { Id = id };

            var result = await sender.Send(command);

            if (result.IsFailure)
            {
                return Results.NotFound(result.Error);
            }
            return Results.NoContent();
        }).WithTags("Accounts");
    }
}
