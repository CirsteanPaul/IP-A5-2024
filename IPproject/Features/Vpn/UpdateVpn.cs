using Carter;
using IP.Project.Database;
using IP.Project.Shared;
using MediatR;


namespace IP.Project.Features.Vpn
{
    public static class UpdateVpnInstance
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
                var vpnInstance = await context.Vpns.FindAsync(request.Id, cancellationToken);
                if (vpnInstance == null)
                {
                    return Result.Failure<Guid>(new Error("UpdateVpn.Null", $"Vpn instance with ID {request.Id} not found."));
                }

                vpnInstance.IPv4Address = request.NewIpAddress;
                if (request.NewDescription != null) { vpnInstance.Description = request.NewDescription; }
                await context.SaveChangesAsync(cancellationToken);

                return Result.Success(request.Id);
            }
        }
    }
    public class UpdateVpnEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("api/v1/vpns/update/{id}", async (Guid id, string newIpAddress, string? newDescription, ISender sender) =>
            {
                var command = new UpdateVpnInstance.Command(id, newIpAddress, newDescription);
                var result = await sender.Send(command);
                if (result.IsSuccess)
                {
                    return Results.NoContent();
                }
                return Results.NotFound(result.Error);
            }).WithTags("vpn");
        }
    }
}

