using Carter;
using FluentValidation;
using IP.Project.Database;
using IP.Project.Shared;
using MediatR;
using IP.Project.Extensions;
using Microsoft.EntityFrameworkCore;

namespace IP.Project.Features.Samba
{
    public class UpdateSambaRequest
    {
        public string NewIpAddress { get; set; }
        public string? NewDescription { get; set; }
    }

    public static class UpdateSambaInstance
    {
        public record Command(Guid Id, UpdateSambaRequest Request) : IRequest<Result<Guid>>
        {
            public class Validator : AbstractValidator<Command>
            {
                public Validator()
                {
                    RuleFor(x => x.Request.NewIpAddress).NotEmpty().IpAddress(); // Using Matricol() for IP address validation
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
                if (request.Request.NewDescription != null) { sambaInstance.Description = request.Request.NewDescription; }
                await context.SaveChangesAsync(cancellationToken);

                return Result.Success(request.Id);
            }
        }
    }

    public class UpdateSambaEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("api/v1/sambas/{id}", async (Guid id, string newIpAddress, string? newDescription, ISender sender) =>
            {
                var request = new UpdateSambaRequest { NewIpAddress = newIpAddress, NewDescription = newDescription };
                var command = new UpdateSambaInstance.Command(id, request);
                var result = await sender.Send(command);
                if (result.IsSuccess)
                {
                    return Results.NoContent();
                }
                return Results.NotFound(result.Error);
            }).WithTags("Samba");
        }
    }
}
