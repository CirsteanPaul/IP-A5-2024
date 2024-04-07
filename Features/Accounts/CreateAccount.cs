using Carter;
using FluentValidation;
using IP.Project.Contracts;
using IP.Project.Database;
using IP.Project.Entities;
using IP.Project.Features.Accounts;
using IP.Project.Shared;
using Mapster;
using MediatR;

namespace IP.Project.Features.Accounts
{
    public static class CreateAccount
    {
        public record Command : IRequest<Result<Guid>>
        {
            public string Username { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Matricol { get; set; } = string.Empty;    
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Username).NotEmpty();
                RuleFor(x => x.Matricol).NotEmpty();
                RuleFor(x => x.Email).NotEmpty();
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
                    Email = request.Email,
                    Matricol = request.Matricol,
                    CreatedOnUtc = DateTime.UtcNow
                };

                //dbContext.Accounts.Add(account);
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
        _ = app.MapPost("api/accounts", async (CreateAccountRequest request, ISender sender) =>
        {
            var command = request.Adapt<CreateAccount.Command>();
            var result = await sender.Send(command);
            if (result.IsFailure)
            {
                return Results.BadRequest(result.Error);
            }
            return Results.Ok($"/api/accounts/{result.Value}");
        });
    }
}
