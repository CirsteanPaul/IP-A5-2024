using Carter;
using FluentValidation;
using IP.Project.Database;
using IP.Project.Features.Vpn;
using IP.Project.Shared;
using MediatR;

namespace IP.Project.Features.Vpn
{
    public static class DeleteVpn
    {
        public record Command(Guid Id) : IRequest<Result>;

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Id).NotEmpty();
            }
        }

        public class Handler : IRequestHandler<Command, Result>
        {
            private readonly ApplicationDBContext dbContext;
            private readonly IValidator<Command> validator;

            public Handler(ApplicationDBContext dbContext, IValidator<Command> validator)
            {
                this.dbContext = dbContext;
                this.validator = validator;
            }

            public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
            {
                var validationResult = validator.Validate(request);
                if (!validationResult.IsValid)
                {
                    return Result.Failure<Guid>(new Error("DeleteVpn.Validator", validationResult.ToString()!));
                }
                var vpn = await dbContext.Vpns.FindAsync(request.Id);
                if (vpn == null)
                {
                    return Result.Failure<Guid>(new Error("DeleteVpn.Null", $"Vpn with id {request.Id} not found"));
                }
                dbContext.Vpns.Remove(vpn);
                await dbContext.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
        }
    }
}

public class DeleteVpnEndpoint :ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        _ = app.MapDelete("api/v1/vpns/{id}", async (Guid id, ISender sender) =>
        {
            var command = new DeleteVpn.Command(id);
            var result = await sender.Send(command);
            if (result.IsFailure)
            {
                if (result.Error.Code == "DeleteVpn.Null")
                {
                    return Results.NotFound(result.Error);
                }
                return Results.BadRequest(result.Error);
            }
            return Results.NoContent();
        })
        .WithTags("Vpn");
    }   
}
