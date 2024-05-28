using Carter;
using FluentValidation;
using IP.Project.Constants;
using IP.Project.Contracts.Samba;
using IP.Project.Database;
using IP.Project.Entities;
using IP.Project.Extensions;
using IP.Project.Features.Samba;
using IP.Project.Shared;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IP.Project.Features.Samba
{
    public static class CreateSamba
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
                    RuleFor(x => x.IPv4Address).NotEmpty().IpAddress(); // Using IpAddress() for IP address validation
                    RuleFor(x => x.Description).NotEmpty().MinimumLength(10);
                }
            }
        
        public class Handler : IRequestHandler<Command, Result<Guid>>
        {
            private readonly ApplicationDBContext dbContext;
            private readonly IValidator<Command> validator;

            public Handler(ApplicationDBContext dbContext, IValidator<Command> validator)
            {
                this.dbContext = dbContext;
                this.validator = validator;
            }

            public async Task<Result<Guid>> Handle(Command request, CancellationToken cancellationToken)
            {
                var validationResult = validator.Validate(request);
                
                if (!validationResult.IsValid)
                {
                    return Result.Failure<Guid>(
                        new Error("CreateSamba.Validator", 
                        validationResult.ToString()!));
                }
                
                var samba = new SambaAccount
                {
                    Description = request.Description,
                    IPv4Address = request.IPv4Address
                };

                dbContext.SambaAccounts.Add(samba);
                await dbContext.SaveChangesAsync(cancellationToken);

                return samba.Id;
            }
        }
    }
}

public class CreateSambaEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        _ = app.MapPost($"{Global.version}sambas", [Authorize(Roles = Roles.Admin)] async ([FromBody] CreateSambaRequest request, ISender sender) =>
            {
                var command = request.Adapt<CreateSamba.Command>();
                var result = await sender.Send(command);

                if (result.IsFailure)
                {
                    return Results.BadRequest(result.Error);
                }

                return Results.Created($"{Global.version}sambas/{result.Value}", null);
            })
            .WithTags("Samba")
            .WithDescription("Endpoint for creating a new samba account. " +
                             "If the request succeeded in the location header you can find the endpoint to get the new account.")
            .Produces(StatusCodes.Status201Created)
            .Produces<Error>(StatusCodes.Status400BadRequest)
            .WithOpenApi();
    }
}
