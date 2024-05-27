using IP.Project.Contracts;
using Carter;
using FluentValidation;
using IP.Project.Database;
using IP.Project.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
namespace IP.Project.Features.Accounts
{   
    public class UpdateAccountInstance
    {
        public record Command(Guid Id, UpdateAccountRequest Request) : IRequest<Result<Guid>>
        {
            public class Validator : AbstractValidator<Command>
            {
                public Validator()
                {
                    RuleFor(x => x.Request.NewEmail).EmailAddress().When(x => x.Request.NewEmail != null);
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
                return Result.Failure<Guid>(new Error("UpdateAccount.ValidationFailed", string.Join(" ", errorMessages)));
             }

            var accountInstance = await context.Accounts.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (accountInstance == null)
            {
                return Result.Failure<Guid>(new Error("UpdateAccount.Null", $"Account instance with ID {request.Id} not found."));
            }

            if (request.Request.NewUsername != null) { accountInstance.Username = request.Request.NewUsername; }
            if (request.Request.NewPassword != null) { accountInstance.Password = request.Request.NewPassword; }
            if (request.Request.NewEmail != null) { accountInstance.Email = request.Request.NewEmail; }

            accountInstance.LastUpdatedOnUtc = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);
            return Result.Success(request.Id);
        }
    }
        }

        public class UpdateAccountEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(Global.version + "accounts/{id:guid}", async ([FromRoute] Guid id, [FromBody] UpdateAccountRequest request, ISender sender) =>
            {
                var command = new UpdateAccountInstance.Command(id, request);
                var result = await sender.Send(command);
                if (result.IsFailure)
                {
                    return Results.NotFound(result.Error);
                }
                return Results.Ok(Global.version + $"accounts/{result.Value}");
            }).WithTags("Accounts")
            .WithDescription("Endpoint for updating an account by id. " +  "If the request succeeds, the updated account id will be returned.")
            .Produces<Guid>(StatusCodes.Status200OK)
            .Produces<Error>(StatusCodes.Status404NotFound)
            .WithOpenApi();
        }
    }
}
