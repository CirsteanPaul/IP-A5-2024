using Carter;
using FluentValidation;
using IP.Project.Contracts;
using IP.Project.Database;
using IP.Project.Entities;
using IP.Project.Features.Accounts;
using IP.Project.Extensions;
using IP.Project.Shared;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IP.Project.Features.Accounts
{
    public static class CreateAccount
    {
        public record Command : IRequest<Result<Guid>>
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Matricol { get; set; } = string.Empty;    
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Username).NotEmpty();
                RuleFor(x => x.Password).NotEmpty();
                RuleFor(x => x.Matricol).NotEmpty().Matricol();
                RuleFor(x => x.Email).NotEmpty().EmailAddress();
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
                           new Error("CreateAccount.Validator", 
                                                  validationResult.ToString()!));
                }
                var account = new Account
                {
                    Username = request.Username,
                    Password = request.Password,
                    Email = request.Email,
                    Matricol = request.Matricol,
                    CreatedOnUtc = DateTime.UtcNow
                };

                dbContext.Accounts.Add(account);
                await dbContext.SaveChangesAsync(cancellationToken);

                return account.Id;
            }
        }

    }
}

public class CreateAccountEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        _ = app.MapPost(Global.version + "accounts", async ([FromBody] CreateAccountRequest request, ISender sender) =>
        {
            var command = request.Adapt<CreateAccount.Command>();
            var result = await sender.Send(command);
            if (result.IsFailure)
            {
                return Results.BadRequest(result.Error);
            }
            return Results.Created(Global.version + $"accounts/{result.Value}", result.Value);
        }).WithTags("Accounts")
        .WithDescription("Endpoint for creating a new account. " +
                                 "If the request succeeds, in the location header you can find the endpoint to get the new account.")
        .Produces<Guid>(StatusCodes.Status201Created)
        .Produces<Error>(StatusCodes.Status400BadRequest)
        .WithOpenApi();
    }
}
