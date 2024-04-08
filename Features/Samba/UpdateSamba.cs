using Carter;
using IP.Project.Database;
using IP.Project.Features.Samba;
using IP.Project.Shared;
using MediatR;

namespace IP.Project.Features.Samba
{
    public static class UpdateSambaDescription
    {
        public record Command(Guid Id, string NewDescription) : IRequest<Result<Guid>>;
        public class Handler : IRequestHandler<Command, Result<Guid>>
        {
            private readonly ApplicationDBContext context;

            public Handler(ApplicationDBContext dbContext)
            {
                this.context = dbContext;
            }

            public async Task<Result<Guid>> Handle(Command request, CancellationToken cancellationToken)
            {
                var sambaInstance = await context.SambaAccounts.FindAsync(new object[] { request.Id }, cancellationToken);
                if (sambaInstance == null)
                {
                    return Result.Failure<Guid>(new Error("UpdateSamba.Null", $"Samba instance with ID {request.Id} not found."));
                }

                sambaInstance.Description = request.NewDescription;
                await context.SaveChangesAsync(cancellationToken);

                return Result.Success(request.Id);
            }
        }
    }

    public static class UpdateSambaIpAddress
    {
        public record Command(Guid Id, string NewIpAddress) : IRequest<Result<Guid>>;

        public class Handler : IRequestHandler<Command, Result<Guid>>
        {
            private readonly ApplicationDBContext context;

            public Handler(ApplicationDBContext dbContext)
            {
                this.context = dbContext;
            }

            public async Task<Result<Guid>> Handle(Command request, CancellationToken cancellationToken)
            {
                var sambaInstance = await context.SambaAccounts.FindAsync(new object[] { request.Id }, cancellationToken);
                if (sambaInstance == null)
                {
                    return Result.Failure<Guid>(new Error("UpdateSamba.Null", $"Samba instance with ID {request.Id} not found."));
                }

                sambaInstance.IPv4Address = request.NewIpAddress;
                await context.SaveChangesAsync(cancellationToken);

                return Result.Success(request.Id);
            }
        }
    }

    public class UpdateSambaEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("api/samba/ipaddress/{id}", async (Guid id, string newIpAddress, ISender sender) =>
            {
                var command = new UpdateSambaIpAddress.Command(id, newIpAddress);
                var result = await sender.Send(command);
                if (result.IsSuccess)
                {
                    return Results.Ok($"Samba IP address updated successfully for ID: {result.Value}");
                }
                return Results.NotFound(result.Error);
            });

            app.MapPut("api/samba/description/{id}", async (Guid id, string newDescription, ISender sender) =>
            {
                var command = new UpdateSambaDescription.Command(id, newDescription);
                var result = await sender.Send(command);
                if (result.IsSuccess)
                {
                    return Results.Ok($"Samba description updated successfully for ID: {result.Value}");
                }
                return Results.NotFound(result.Error);
            });
        }
    }
}
