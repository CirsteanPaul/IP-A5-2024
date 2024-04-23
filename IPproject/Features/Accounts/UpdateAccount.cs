using Carter;
using IP.Project.Database;
using IP.Project.Shared;
using MediatR;

namespace IP.Project.Features.Accounts
{
    public class UpdateAccountInstance
    {
        public record Command(Guid Id, string? NewUsername, string? NewPassword, string? NewEmail) : IRequest<Result<Guid>>;
        public class Handler : IRequestHandler<Command, Result<Guid>>
        {
            private readonly ApplicationDBContext context;

            public Handler(ApplicationDBContext dbContext)
            {
                this.context = dbContext;
            }

            public async Task<Result<Guid>> Handle(Command request, CancellationToken cancellationToken)
            {
                var accountInstance = await context.Accounts.FindAsync(request.Id, cancellationToken);
                if (accountInstance == null)
                {
                    return Result.Failure<Guid>(new Error("UpdateAccount.Null", $"Account instance with ID {request.Id} not found."));
                }

                if (request.NewUsername != null) { accountInstance.Username = request.NewUsername; }
                if (request.NewPassword != null) { accountInstance.Password = request.NewPassword; }
                if (request.NewEmail != null) { accountInstance.Email = request.NewEmail; }

                accountInstance.LastUpdatedOnUtc = DateTime.UtcNow;

                await context.SaveChangesAsync(cancellationToken);
                return Result.Success(request.Id);
            }
        }
    }

    public class UpdateAccountEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(Global.version + "accounts/update/{id}", async (Guid id, string? Username, string? Password, string? Email, ISender sender) =>
            {
                var command = new UpdateAccountInstance.Command(id, Username, Password, Email);
                var result = await sender.Send(command);
                if (result.IsFailure)
                {
                    return Results.NotFound(result.Error);
                }
                return Results.Ok(Global.version + $"accounts/{result.Value}");
            }).WithTags("Accounts"); ;
        }
    }
}
