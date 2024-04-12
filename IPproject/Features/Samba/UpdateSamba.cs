using Carter;
using IP.Project.Database;
using IP.Project.Shared;
using MediatR;

namespace IP.Project.Features.Samba
{
    public static class UpdateSambaInstance
    {
        public record Command(Guid Id, string NewIpAddress, string? NewDescription) : IRequest<Result<Guid>>;
        public class Handler : IRequestHandler<Command, Result<Guid>>
        {
            private readonly ApplicationDBContext context;

            public Handler(ApplicationDBContext dbContext)
            {
                this.context = dbContext;
            }

            public async Task<Result<Guid>> Handle(Command request, CancellationToken cancellationToken)
            {
                var sambaInstance = await context.SambaAccounts.FindAsync(request.Id, cancellationToken);
                if (sambaInstance == null)
                {
                    return Result.Failure<Guid>(new Error("UpdateSamba.Null", $"Samba instance with ID {request.Id} not found."));
                }

                sambaInstance.IPv4Address = request.NewIpAddress;
                if (request.NewDescription != null) { sambaInstance.Description = request.NewDescription; }
                await context.SaveChangesAsync(cancellationToken);

                return Result.Success(request.Id);
            }
        }
    }

    public class UpdateSambaEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("api/samba/update/{id}", async (Guid id, string newIpAddress, string? newDescription, ISender sender) =>
            {
                var command = new UpdateSambaInstance.Command(id, newIpAddress, newDescription);
                var result = await sender.Send(command);
                if (result.IsSuccess)
                {
                    return Results.Ok($"/api/samba/{result.Value}");
                }
                return Results.NotFound(result.Error);
            });
        }
    }
}
