﻿using Carter;
using FluentValidation;
using IP.Project.Constants;
using IP.Project.Contracts.Vpn;
using IP.Project.Database;
using IP.Project.Extensions;
using IP.Project.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

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
                    RuleFor(x => x.Request.NewDescription).NotEmpty().TrimmedMinLength(10); 
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
                var validationResult = new Command.Validator().Validate(request);
                var errorMessages = validationResult.Errors
                    .Select(error => error.ErrorMessage)
                    .ToList();
                if (!validationResult.IsValid)
                {
                    return Result.Failure<Guid>(new Error("UpdateVpn.ValidationFailed", string.Join(" ", errorMessages)));
                }

                var vpnInstance = await context.Vpns.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

                if (vpnInstance == null)
                {
                    return Result.Failure<Guid>(new Error("UpdateVpn.Null", $"Vpn instance with ID {request.Id} not found."));
                }

                vpnInstance.IPv4Address = request.Request.NewIpAddress;
                vpnInstance.Description = request.Request.NewDescription;
                
                await context.SaveChangesAsync(cancellationToken);

                return Result.Success(request.Id);
            }
        }
    }
    public class UpdateVpnEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut($"{Global.Version}vpns/{{id}}", [Authorize(Roles = Roles.Admin)] async (Guid id, UpdateVpnRequest request, ISender sender) =>
            {
                var command = new UpdateVpnInstance.Command(id, request);
                var result = await sender.Send(command);
                
                return result.IsSuccess ? Results.NoContent() : Results.NotFound(result.Error);
            })
            .WithTags("Vpn");
        }
    }
}

