using Carter;
using FluentValidation;
using IP.Project.Contracts;
using IP.Project.Database;
using IP.Project.Features.Vpn;
using IP.Project.Shared;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace IP.Project.Features.Vpn
{
    public static class CreateVpn
    {
        public record Command : IRequest<Result<Guid>>
        {
            public string? Description { get; set; }
            public string IPv4Address { get; set; } = string.Empty;
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Description).NotEmpty();
                RuleFor(x => x.IPv4Address).NotEmpty()
                    .MaximumLength(16).WithMessage("IPv4 address exceeds maximum length of 15 characters")
                    .Matches(@"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$").WithMessage("Invalid IPv4 address format");
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
                        new Error("CreateVpn.Validator", validationResult.ToString()));
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
        _ = app.MapPost("api/v1/vpns", async (CreateVpnRequest request, ISender sender) =>
        {
            var command = request.Adapt<CreateVpn.Command>();
            var result = await sender.Send(command);

            if (result.IsFailure)
            {
                return Results.BadRequest(result.Error); 
            }

            return Results.Created($"/api/v1/vpns/{result.Value}", result.Value); 
        });
    }
}
