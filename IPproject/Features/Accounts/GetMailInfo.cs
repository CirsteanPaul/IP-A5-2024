using Azure.Core;
using Carter;
using IP.Project.Contracts;
using IP.Project.Database;
using IP.Project.Entities;
using IP.Project.Features.Accounts;
using IP.Project.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.OpenApi.Any;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Numerics;
using System.Threading;

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

            public async Task<Result<int>> addPartialEntryToDb(string matricol, CancellationToken cancellationToken)
            {
                var uidNumber = await generateUidNumber(cancellationToken);

                var account = new Account
                {
                    Username = "", // for old endpoints
                    Password = "", // for old endpoints
                    Email = "", // for old endpoints
                    Matricol = matricol, // from esims
                    CNP = "", // from esims
                    cn = "Ion Popescu", // from esims
                    sn = "Popescu", // from esims
                    gidNumber = uidNumber, // same value as uidNumber, uniquely generated between 2000 and 7999
                    uidNumber = uidNumber, // same value as gidNumber, uniquely generated between 2000 and 7999
                    uid = "", // part of email
                    homeDirectory = "", // /home/username(uid)
                    displayName = "Ion Popescu", // from esims
                    employeeNumber = "",
                    givenName = "Ion", // from esims
                    homePhone = "",
                    initials = "",
                    localityName = "Iasi", // from esims
                    mail = "", // will be chosen
                    mobile = "", // will be chosen
                    ou = "1A1", // from esims
                    postalCode = "",
                    roomNumber = "",
                    shadowInactive = "",
                    street = "",
                    telephoneNumber = "",
                    title = "",
                    userPassword = "", // will be chosen
                    CreatedOnUtc = DateTime.UtcNow,
                    LastUpdatedOnUtc = DateTime.UtcNow
                };

                account.initials = account.givenName[0] + account.sn[0].ToString();



                dbContext.Accounts.Add(account);
                await dbContext.SaveChangesAsync(cancellationToken);

                return account.uidNumber;
            }

            public async Task<List<String>> generateMailVariants(string firstName, string lastName, CancellationToken cancellationToken)
            {
                var accounts = from a in dbContext.Accounts
                            where a.givenName.Equals(firstName) && a.sn.Equals(lastName)
                            select a;

                var mailVariants = new List<String>();
                var validVariants = new List<String>();

                var first = firstName.ToLower();
                var last = lastName.ToLower();

                mailVariants.Add(first + "." + last + "@info.uaic.ro");
                mailVariants.Add(first + "." + last[0] + "@info.uaic.ro");
                mailVariants.Add(first[0] + "." + last + "@info.uaic.ro");
                mailVariants.Add(first[0..2] + "." + last + "@info.uaic.ro");
                mailVariants.Add(first + "." + last[0..2] + "@info.uaic.ro");
                mailVariants.Add(first + "." + last + '1' + "@info.uaic.ro");
                mailVariants.Add(first + "." + last[0] + '1' +  "@info.uaic.ro");
                mailVariants.Add(first[0] + "." + last + '1' +"@info.uaic.ro");
                mailVariants.Add(first[0..2] + "." + last + '1' + "@info.uaic.ro");
                mailVariants.Add(first + "." + last[0..2] + '1' + "@info.uaic.ro");

                var counter = 0;

                while(counter < 3)
                {
                    foreach(var mail in mailVariants)
                    {
                        var ok = true;
                        foreach(var account in accounts)
                        {
                            if(mail == account.mail)
                            {
                                ok = false;
                                break;
                            }
                        }
                        if (ok)
                        {
                            counter++;
                            validVariants.Add(mail);
                        }
                        if (counter == 3)
                        {
                            break;
                        }
                    }
                }

                return validVariants;
            }

            public async Task<int> generateUidNumber(CancellationToken cancellationToken)
            {
                var uid = dbContext.Accounts.Max(x => x.uidNumber);

                if(uid == 0)
                {
                    return 2000;
                }
                //problem for future devs
                else if(uid == 7999)
                {
                    return 2000;
                }
                else
                {
                    return uid + 1;
                }   
            }

            public async Task<Result<MailInfoResponse>> Handle(Query request, CancellationToken cancellationToken)
            {
                // firstly search in our database
                var accountInstance = await dbContext.Accounts.FirstOrDefaultAsync(x => x.Matricol == request.Matricol, cancellationToken);

                if (accountInstance == null)
                {
                    //if it does not exist in esims, return error
                    //return Result.Failure<MailInfoResponse>(
                    //                           new Error("GetMailInfo.Null", $"Account instance with Matricol {request.Matricol} not found."));

                    //search in esims for this matricol
                    //suppose we found the user in esims
                    var uidNumber = await addPartialEntryToDb(request.Matricol, cancellationToken);

                    var newAccount = await dbContext.Accounts.FirstOrDefaultAsync(x => x.uidNumber == uidNumber.Value, cancellationToken);

                    var mailVariants = await generateMailVariants(newAccount.givenName, newAccount.sn, cancellationToken);

                    var response = new MailInfoResponse
                    {
                        UidNumber = newAccount.uidNumber,
                        FirstName = newAccount.givenName,
                        LastName = newAccount.sn,
                        MailVariant1 = mailVariants[0],
                        MailVariant2 = mailVariants[1],
                        MailVariant3 = mailVariants[2]
                    };

                    return response;

                }
                else
                {

                    var mailVariants = await generateMailVariants(accountInstance.givenName, accountInstance.sn, cancellationToken);

                    var response = new MailInfoResponse
                    {
                        UidNumber = accountInstance.uidNumber,
                        FirstName = accountInstance.givenName,
                        LastName = accountInstance.sn,
                        MailVariant1 = mailVariants[0],
                        MailVariant2 = mailVariants[1],
                        MailVariant3 = mailVariants[2]
                    };

                    return response;
                }
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
        .WithDescription("Endpoint for getting mail variants by user information. " +
        "If the request succeeds, in the response body you can find the mail address.")
        .Produces<AccountResponse>(StatusCodes.Status200OK)
        .Produces<Error>(StatusCodes.Status404NotFound)
        .WithOpenApi();
    }
}
