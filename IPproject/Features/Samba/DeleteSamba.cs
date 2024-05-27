using Carter;
using IP.Project.Database;
using IP.Project.Resources;
using IP.Project.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace IP.Project.Features.Samba
{
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
                var sambaAccountToBeDeleted = await dbContext.SambaAccounts
                    .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

                if (sambaAccountToBeDeleted is null)
                {
                    return Result.Failure(new Error("DeleteSamba.NotFound", "Samba account not found"));
                }

                dbContext.SambaAccounts.Remove(sambaAccountToBeDeleted);
                await dbContext.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
        }
    }
    public class DeleteSambaEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete($"{Global.version}sambas/{{id}}", [Authorize(Roles = UserRoles.Admin)] async (Guid id, ISender sender) =>
                {
                    var command = new DeleteSamba.Command(id);
                    var result = await sender.Send(command);

                    return result.IsSuccess ? Results.NoContent() : Results.NotFound(result.Error);
                })
                .WithTags("Samba")
                .Produces(StatusCodes.Status204NoContent)
                .Produces<Error>(StatusCodes.Status404NotFound)
                .WithDescription("Endpoint for deleting a specific Samba account. " +
                                 "If the request is successful, it will return status code 204 (No content).")
                .WithOpenApi();
        }
    }
}
