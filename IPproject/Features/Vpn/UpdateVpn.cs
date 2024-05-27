using Carter;
using FluentValidation;
using IP.Project.Contracts;
using IP.Project.Database;
using IP.Project.Extensions;
using IP.Project.Resources;
using IP.Project.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace IP.Project.Features.Vpn
{
    public static class UpdateVpnInstance
    {
        public record Command(Guid Id, UpdateVpnRequest Request) : IRequest<Result<Guid>>
        {
            public class Validator : AbstractValidator<Command>
            {
                public Validator()
                {
                    RuleFor(x => x.Request.NewIpAddress).NotEmpty().IpAddress(); 
                }
            }
        }
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

                vpnInstance.IPv4Address = request.Request.NewIpAddress;
                if (request.Request.NewDescription != null) { vpnInstance.Description = request.Request.NewDescription; }
                await context.SaveChangesAsync(cancellationToken);

                return Result.Success(request.Id);
            }
        }
    }
    public class UpdateVpnEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("api/v1/vpns/{id}", [Authorize(Roles = UserRoles.Admin)] async (Guid id, UpdateVpnRequest request, ISender sender) =>
            {
                var command = new UpdateVpnInstance.Command(id, request);
                var result = await sender.Send(command);
                if (result.IsSuccess)
                {
                    return Results.NoContent();
                }
                return Results.NotFound(result.Error);
            })
            .WithTags("Vpn");
        }
    }
}

