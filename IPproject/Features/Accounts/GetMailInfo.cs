using Carter;
using IP.Project.Contracts;
using IP.Project.Database;
using IP.Project.Features.Accounts;
using IP.Project.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IP.Project.Features.Accounts
{
    public static class GetMailInfo
    {
        public record Query : IRequest<Result<MailInfoResponse>>
        {
            public string Matricol { get; set; } = string.Empty;
        }

        internal sealed class Handler : IRequestHandler<Query, Result<MailInfoResponse>>
        {
            private readonly ApplicationDBContext dbContext;

            public Handler(ApplicationDBContext dbContext)
            {
                this.dbContext = dbContext;
            }
            public async Task<Result<MailInfoResponse>> Handle(Query request, CancellationToken cancellationToken)
            {
               // check in esims if the matricol and CNP are valid

               var response = new MailInfoResponse 
                {
                    FirstName = "Alex", // account.FirstName,
                    LastName = "Popescu", // account.LastName,
                    Mail = "alex.popescu@info.uaic.ro" // account.Mail
                };

                return response;
            }
        }
    }
}

public class GetMailInfoEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(Global.version + "accounts/mail/{matricol}", async ([FromRoute] String matricol, ISender sender) =>
        {
            var query = new GetMailInfo.Query
            {
                Matricol = matricol
            };
            var result = await sender.Send(query);
            if (result.IsFailure)
            {
                return Results.NotFound(result.Error);
            }
            return Results.Ok(result.Value);
        }).WithTags("Accounts")
        .WithDescription("Endpoint for getting a mail variant by user information. " +
        "If the request succeeds, in the response body you can find the mail address.")
        .Produces<AccountResponse>(StatusCodes.Status200OK)
        .Produces<Error>(StatusCodes.Status404NotFound)
        .WithOpenApi();
    }
}
