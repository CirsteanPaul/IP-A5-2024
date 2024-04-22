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
using Microsoft.AspNetCore.Http;

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
                    Id = Guid.NewGuid(),
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
        _ = app.MapPost("api/v1/accounts", async (CreateAccountRequest request, ISender sender) =>
        {
            var command = request.Adapt<CreateAccount.Command>();
            var result = await sender.Send(command);
            if (result.IsFailure)
            {
                return Results.BadRequest(result.Error);
            }
            return Results.Created($"/api/v1/accounts/{result.Value}", result.Value);
        }).WithTags("Accounts");
    }
}
