using Carter;
using IP.Project.Contracts;
using IP.Project.Database;
using IP.Project.Features.Accounts;
using IP.Project.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IP.Project.Features.Accounts
{
    public static class GetAccount
    {
        public record Query : IRequest<Result<AccountResponse>>
        {
            public Guid Id { get; set; }
        }

        internal sealed class Handler : IRequestHandler<Query, Result<AccountResponse>>
        {
            private readonly ApplicationDBContext dbContext;

            public Handler(ApplicationDBContext dbContext)
            {
                this.dbContext = dbContext;
            }
            public async Task<Result<AccountResponse>> Handle(Query request, CancellationToken cancellationToken)
            {
                var account = await dbContext.Accounts
                    .AsNoTracking()
                    .Where(account => account.Id == request.Id)
                    .FirstOrDefaultAsync(cancellationToken);
                if (account is null)
                {
                    return Result.Failure<AccountResponse>(
                        new Error("GetAccount.Null", "Account not found"));
                }

                var accountResponse = new AccountResponse
                {
                    Id = account.Id,
                    Username = account.Username,
                    Password = account.Password,
                    Email = account.Email,
                    Matricol = account.Matricol,
                    CreatedOnUtc = account.CreatedOnUtc,
                    LastUpdatedOnUtc = account.LastUpdatedOnUtc
                };

                return accountResponse;
            }
        }
    }
}

public class GetAccountEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(Global.version + "accounts/{id:guid}", async (Guid id, ISender sender) =>
        {
            var query = new GetAccount.Query
            {
                Id = id
            };
            var result = await sender.Send(query);
            if (result.IsFailure)
            {
                return Results.NotFound(result.Error);
            }
            return Results.Ok(result.Value);
        }).WithTags("Accounts"); ;
    }
}