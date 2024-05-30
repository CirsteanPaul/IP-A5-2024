using Carter;
using IP.Project.Contracts;
using IP.Project.Database;
using IP.Project.Entities;
using IP.Project.Features.Accounts;
using IP.Project.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.Collections.Specialized;
using Novell.Directory.Ldap;
using FluentValidation;
using IP.Project.Extensions;

namespace IP.Project.Features.Accounts
{
    public static class GetMailInfo
    {
        public record Query : IRequest<Result<MailInfoResponse>> 
        {
            public string Matricol { get; set; } = string.Empty;
            public class Validator : AbstractValidator<Query>
            {
                public Validator()
                {
                    RuleFor(x => x.Matricol).NotEmpty().Matricol();
                }
            }
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

                //add partial entry to db
                var uidNumber = await generateUidNumber(cancellationToken);

                var account = new Account
                {
                    Username = "", // for old endpoints
                    Password = "", // for old endpoints
                    Email = "", // for old endpoints
                    Matricol = matricol, // from esims
                    CNP = "", // from esims
                    description = "Student", // will be chosen?
                    cn = "Ion Popescu", // from esims
                    sn = "Popescu", // from esims
                    gidNumber = uidNumber, // same value as uidNumber, uniquely generated between 2000 and 7999
                    uidNumber = uidNumber, // same value as gidNumber, uniquely generated between 2000 and 7999
                    uid = "to.be", // part of email
                    homeDirectory = "/home/to.be", // /home/username(uid)
                    displayName = "Ion Popescu", // from esims
                    employeeNumber = "111",
                    givenName = "Ion", // from esims
                    homePhone = "0230000000",
                    initials = "",
                    localityName = "Iasi", // from esims
                    mail = "to.be@info.uaic.ro", // will be chosen
                    mobile = "0230000000", // will be chosen
                    ou = "2A1", // from esims
                    postalCode = "70000",
                    roomNumber = "1",
                    shadowInactive = "0",
                    street = "Iasi",
                    telephoneNumber = "0230000000",
                    title = "Mr.",
                    userPassword = "", // will be chosen
                    CreatedOnUtc = DateTime.UtcNow,
                    LastUpdatedOnUtc = DateTime.UtcNow
                };

                account.initials = account.givenName[0] + account.sn[0].ToString();

                dbContext.Accounts.Add(account);
                await dbContext.SaveChangesAsync(cancellationToken);

                //add partial entry to ldap
                string ldapServer = "localhost";
                int ldapPort = 10389;
                string adminUserName = "uid=admin,ou=system";
                string adminPassword = "secret";
                string baseDn = "dc=info,dc=uaic,dc=ro";

                try
                {
                    using (var ldapConnection = new LdapConnection())
                    {
                        // Connect and authenticate
                        ldapConnection.Connect(ldapServer, ldapPort);
                        ldapConnection.Bind(adminUserName, adminPassword);
                        Console.WriteLine("Authenticated");

                        // Search for the organizational unit
                        string ouDn = $"ou=license,ou=students,{baseDn}";
                        string searchFilter = $"(&(objectClass=organizationalUnit)(ou={account.ou}))";
                        var searchResults = ldapConnection.Search(
                            ouDn,
                            LdapConnection.ScopeSub,
                            searchFilter,
                            null, // retrieve all attributes
                            false
                        );

                        if (!searchResults.HasMore())
                        {
                            // Create a new organizational unit entry if not found
                            var newOUDn = $"ou={account.ou},{ouDn}";
                            var newOUAttributes = new LdapAttributeSet
                    {
                        new LdapAttribute("objectClass", "organizationalUnit"),
                        new LdapAttribute("ou", account.ou)
                    };
                            var newOUEntry = new LdapEntry(newOUDn, newOUAttributes);
                            ldapConnection.Add(newOUEntry);
                            Console.WriteLine("Organizational unit added successfully.");
                        }

                        // Create a new user entry
                        var userDn = $"cn={account.displayName},ou={account.ou},{ouDn}";
                        var userAttributes = new LdapAttributeSet
                {
                    new LdapAttribute("objectClass", new [] { "inetOrgPerson", "organizationalPerson", "person", "posixAccount", "shadowAccount" }),
                    new LdapAttribute("sn", account.sn),
                    new LdapAttribute("mail", account.mail),
                    new LdapAttribute("uidNumber", account.uidNumber.ToString()),
                    new LdapAttribute("gidNumber", account.gidNumber.ToString()),
                    new LdapAttribute("uid", account.uid),
                    new LdapAttribute("homeDirectory", account.homeDirectory),
                    new LdapAttribute("displayName", account.displayName),
                    new LdapAttribute("employeeNumber", account.employeeNumber),
                    new LdapAttribute("givenName", account.givenName),
                    new LdapAttribute("homePhone", account.homePhone),
                    new LdapAttribute("initials", account.initials),
                    new LdapAttribute("localityName", new[] { account.localityName, "Suceava" }),
                    new LdapAttribute("mobile", account.mobile),
                    new LdapAttribute("ou", account.ou),
                    new LdapAttribute("postalCode", account.postalCode),
                    new LdapAttribute("roomNumber", account.roomNumber),
                    new LdapAttribute("shadowInactive", account.shadowInactive),
                    new LdapAttribute("street", account.street),
                    new LdapAttribute("telephoneNumber", account.telephoneNumber),
                    new LdapAttribute("title", account.title),
                    new LdapAttribute("description", account.description)
                    //new LdapAttribute("userPassword", account.userPassword) // Uncomment if password needs to be set
                };
                        var newUserEntry = new LdapEntry(userDn, userAttributes);
                        ldapConnection.Add(newUserEntry);
                        Console.WriteLine("User added successfully.");
                    }
                }
                catch (LdapException ex)
                {
                    Console.WriteLine("LDAP Error: " + ex.Message);
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine("Inner exception: " + ex.InnerException.Message);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }

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
                var validationResult = new Query.Validator().Validate(request);
                if (!validationResult.IsValid)
                {
                    var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
                    return Result.Failure<MailInfoResponse>(new Error("GetMailInfo.Validation", validationResult.ToString()));
                }

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
        "If the request succeeds, in the response body you can find the mail addresses.")
        .Produces<MailInfoResponse>(StatusCodes.Status200OK)
        .Produces<Error>(StatusCodes.Status404NotFound)
        .WithOpenApi();
    }
}
