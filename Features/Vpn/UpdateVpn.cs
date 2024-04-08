using Carter;
using IP.Project.Database;
using IP.Project.Shared;
using MediatR;


namespace IP.Project.Features.Vpn
{
    public static class UpdateVpnDescription
    {
        public record Command(Guid Id, string NewDescription) : IRequest<Result<Guid>>;

        public class Handler : IRequestHandler<Command, Result<Guid>>
        {
            private readonly ApplicationDBContext _dbContext;

            public Handler(ApplicationDBContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<Result<Guid>> Handle(Command request, CancellationToken cancellationToken)
            {
                var vpnInstance = await _dbContext.Vpns.FindAsync(new object[] { request.Id }, cancellationToken);
                if (vpnInstance == null)
                {
                    return Result.Failure<Guid>(new Error("NotFound", $"VPN instance with ID {request.Id} not found."));
                }

                vpnInstance.Description = request.NewDescription;
                await _dbContext.SaveChangesAsync(cancellationToken);

                return Result.Success(request.Id);

            }
        }
    }

    public static class UpdateVpnIpAddress
    {
        public record Command(Guid Id, string NewIpAddress) : IRequest<Result<Guid>>;

        public class Handler : IRequestHandler<Command, Result<Guid>>
        {
            private readonly ApplicationDBContext _dbContext;

            public Handler(ApplicationDBContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<Result<Guid>> Handle(Command request, CancellationToken cancellationToken)
            {
                var vpnInstance = await _dbContext.Vpns.FindAsync(new object[] { request.Id }, cancellationToken);
                if (vpnInstance == null)
                {
                    return Result.Failure<Guid>(new Error("NotFound", $"VPN instance with ID {request.Id} not found."));
                }

                vpnInstance.IPv4Address = request.NewIpAddress;
                await _dbContext.SaveChangesAsync(cancellationToken);

                return Result.Success(request.Id);

            }
        }
    }

    public class UpdateVpnEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("api/vpn/description/{id}", async (Guid id, string newDescription, ISender sender) =>
            {
                var command = new UpdateVpnDescription.Command(id, newDescription);
                var result = await sender.Send(command);
                if (result.IsSuccess)
                {
                    return Results.Ok($"VPN description updated successfully for ID: {result.Value}");
                }
                return Results.NotFound(result.Error);
            });

            app.MapPut("api/vpn/ipaddress/{id}", async (Guid id, string newIpAddress, ISender sender) =>
            {
                var command = new UpdateVpnIpAddress.Command(id, newIpAddress);
                var result = await sender.Send(command);
                if (result.IsSuccess)
                {
                    return Results.Ok($"VPN IP address updated successfully for ID: {result.Value}");
                }
                return Results.NotFound(result.Error);
            });

        }
    }
}

