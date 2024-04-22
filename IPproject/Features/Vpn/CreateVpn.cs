using Carter;
using FluentValidation;
using IP.Project.Contracts;
using IP.Project.Database;
using IP.Project.Features.Vpn;
using IP.Project.Shared;
using Mapster;
using MediatR;

namespace IP.Project.Features.Vpn
{
    public static class CreateVpn
    {
        public record Command : IRequest<Result<Guid>>
        {
            public string? Description { get; set; }
            public string IPv4Address { get; set; }  = string.Empty;
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Description).NotEmpty();
                RuleFor(x => x.IPv4Address).MaximumLength(16);
            }
        }

        public class Handler : IRequestHandler<Command, Result<Guid>>
        {
            private readonly ApplicationDBContext _dbContext;
            private readonly IValidator<Command> _validator;

            public Handler(ApplicationDBContext dbContext, IValidator<Command> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
            }

            public async Task<Result<Guid>> Handle(Command request, CancellationToken cancellationToken)
            {
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    return Result.Failure<Guid>(
                        new Error("CreateVpn.Validator",
                        validationResult.ToString()));
                }

                var vpn = new Entities.VpnAccount 
                {
                    Id = Guid.NewGuid(),
                    Description = request.Description,
                    IPv4Address = request.IPv4Address,
                };

                _dbContext.Vpns.Add(vpn);
                await _dbContext.SaveChangesAsync(cancellationToken);

                return vpn.Id;
            }
        }
    }
}


public class CreateVpnEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        _ = app.MapPost("api/vpn", async (CreateVpnRequest request, ISender sender) =>
        {
            var command = request.Adapt<CreateVpn.Command>();
            var result = await sender.Send(command);
            
            if (result.IsFailure)
            {
                return Results.BadRequest(result.Error);
            }
            
            return Results.Ok($"/api/vpn/{result.Value}");
        });
    }
}
