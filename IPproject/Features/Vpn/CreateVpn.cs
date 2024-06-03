using Carter;
using FluentValidation;
using IP.Project.Constants;
using IP.Project.Contracts.Vpn;
using IP.Project.Database;
using IP.Project.Extensions;
using IP.Project.Shared;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace IP.Project.Features.Vpn
{
    public static class CreateVpn
    {
        public record Command : IRequest<Result<Guid>>
        {
            public string Description { get; set; } = string.Empty;
            public string IPv4Address { get; set; } = string.Empty;
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Description).NotEmpty().MinimumLength(10);
                RuleFor(x => x.IPv4Address).NotEmpty().IpAddress();
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
                    var validResult = validationResult.ToString();
                    if(validResult is not null)
                    {return Result.Failure<Guid>(new Error("CreateVpn.Validator", validResult));}
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

    public class CreateVpnEndPoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            _ = app.MapPost($"{Global.Version}vpns", [Authorize(Roles = Roles.Admin)] async (CreateVpnRequest request, ISender sender) =>
            {
                var command = request.Adapt<CreateVpn.Command>();
                var result = await sender.Send(command);

                if (result.IsFailure)
                {
                    return Results.BadRequest(result.Error); 
                }

                return Results.Created($"{Global.Version}vpns/{result.Value}", result.Value);
            })
            .WithTags("Vpn");
        }
    }
}
