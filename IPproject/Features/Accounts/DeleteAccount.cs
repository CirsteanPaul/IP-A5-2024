using Carter;
using IP.Project.Database;
using IP.Project.Entities;
using IP.Project.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IP.Project.Features.Accounts;


public class DeleteAccount
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
            var accountToBeDeleted = await dbContext.Accounts.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

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
        app.MapDelete(Global.version + "accounts/{id:guid}", async ([FromRoute] Guid id, ISender sender) =>
        {
            var command = new DeleteAccount.Command(id);
            var result = await sender.Send(command);

            if (result.IsFailure)
            {
                return Results.NotFound(result.Error);
            }
            return Results.NoContent();
        }).WithTags("Accounts")
        .WithDescription("Endpoint for deleting an account by id. " + "If the account doesn't exist, a not found error will be returned.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<Error>(StatusCodes.Status404NotFound)
        .WithOpenApi();
    }
}
