﻿using Carter;
using FluentValidation;
using IP.Project.Constants;
using IP.Project.Database;
using IP.Project.Shared;
using MediatR;
using IP.Project.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace IP.Project.Features.Samba
{
    public class UpdateSambaRequest
    {
        public string NewIpAddress { get; set; } = string.Empty;
        public string NewDescription { get; set; } = string.Empty;
    }

    public static class UpdateSambaInstance
    {
        public record Command(Guid Id, UpdateSambaRequest Request) : IRequest<Result<Guid>>
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
                    return Result.Failure<Guid>(new Error("UpdateSamba.ValidationFailed", string.Join(" ", errorMessages)));
                }

                var sambaInstance = await context.SambaAccounts.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

                if (sambaInstance == null)
                {
                    return Result.Failure<Guid>(new Error("UpdateSamba.Null", $"Samba instance with ID {request.Id} not found."));
                }

                sambaInstance.IPv4Address = request.Request.NewIpAddress;
                sambaInstance.Description = request.Request.NewDescription;
                
                await context.SaveChangesAsync(cancellationToken);

                return Result.Success(request.Id);
            }
        }
    }

    public class UpdateSambaEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut($"{Global.Version}sambas/{{id:guid}}", [Authorize(Roles = Roles.Admin)] async (Guid id, UpdateSambaRequest request, ISender sender) =>
            {
                var command = new UpdateSambaInstance.Command(id, request);
                var result = await sender.Send(command);
                
                return result.IsSuccess ? Results.NoContent() : Results.NotFound(result.Error);
            }).WithTags("Samba")
            .WithDescription("Endpoint for updating details of a specific Samba account. " +
                         "If the request is successful, it will return status code 204 (No content). ")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<Error>(StatusCodes.Status404NotFound)
            .WithOpenApi();
        }
    }
}
