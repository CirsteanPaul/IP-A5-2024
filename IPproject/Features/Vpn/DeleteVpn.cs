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
        public record Command : IRequest<Result<Guid>>
        {
            public Guid Id { get; set; }
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Id).NotEmpty();
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
                    return Result.Failure<Guid>(new Error("DeleteVpn.Validator", validationResult.ToString()!));
                }
                var vpn = await dbContext.Vpns.FindAsync(request.Id);
                if (vpn == null)
                {
                    return Result.Failure<Guid>(new Error("DeleteVpn.Handler", "Vpn not found"));
                }
                dbContext.Vpns.Remove(vpn);
                await dbContext.SaveChangesAsync(cancellationToken);

                return vpn.Id;
            }
        }
    }
}

public class DeleteVpnEndpoint :ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        _ = app.MapDelete("api/vpn/delete/{id}", async (Guid id, ISender sender) =>
        {
            var command = new DeleteVpn.Command()
            {
                Id = id
            };
            var result = await sender.Send(command);
            if (result.IsFailure)
            {
                return Results.NotFound(result.Error);
            }
            return Results.NoContent();
        }).WithTags("vpn");
    }   
}
