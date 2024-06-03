using Carter;
using FluentValidation;
using IP.Project.Contracts;
using IP.Project.Database;
using IP.Project.Entities;
using IP.Project.Extensions;
using IP.Project.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Novell.Directory.Ldap;

namespace IP.Project.Features.Accounts;

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

    internal sealed class Handler(ApplicationDBContext dbContext, ILdapService ldapService) : IRequestHandler<Query, Result<MailInfoResponse>>
    {
        private readonly ApplicationDBContext dbContext = dbContext;
        internal static readonly string[] objectProperties = ["inetOrgPerson", "organizationalPerson", "person", "posixAccount", "shadowAccount"];
        private readonly string ldapServer = ldapService.GetLdapSettings().LdapServer;
        private readonly int ldapPort = ldapService.GetLdapSettings().LdapPort;
        private readonly string adminUserName = ldapService.GetLdapSettings().AdminUserName;
        private readonly string adminPassword = ldapService.GetLdapSettings().AdminPassword;
        private readonly string baseDn = ldapService.GetLdapSettings().BaseDN;

        public async Task<Result<int>> AddPartialEntryToDb(string matricol, CancellationToken cancellationToken)
        {
            Console.WriteLine("LDAP Server: " + ldapServer);
            Console.WriteLine("LDAP Port: " + ldapPort);
            Console.WriteLine("Admin Username: " + adminUserName);
            Console.WriteLine("Admin Password: " + adminPassword);

            //add partial entry to db
            var uidNumber = await GenerateUidNumber(cancellationToken);

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

            await dbContext.Accounts.AddAsync(account, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            //add partial entry to ldap
            try
            {
                using var ldapConnection = new LdapConnection();
                // Connect and authenticate
                ldapConnection.Connect(ldapServer, ldapPort);
                ldapConnection.Bind(adminUserName, adminPassword);
                Console.WriteLine("LDAP: Authenticated");

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
                    Console.WriteLine("LDAP: Organizational unit added successfully.");
                }

                // Create a new user entry
                var userDn = $"cn={account.displayName},ou={account.ou},{ouDn}";
                var userAttributes = new LdapAttributeSet
            {
                new LdapAttribute("objectClass", objectProperties),
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
                Console.WriteLine("LDAP: User added successfully.");
            }
            catch (LdapException ex)
            {
                Console.WriteLine("LDAP Error: " + ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("LDAP: Inner exception: " + ex.InnerException.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("LDAP: Error: " + ex.Message);
            }

            return account.uidNumber;
        }

        public async Task<List<string>> GenerateMailVariants(string firstName, string lastName, CancellationToken cancellationToken)
        {
            var accounts = await dbContext.Accounts
                              .Where(a => a.givenName.Equals(firstName) && a.sn.Equals(lastName))
                              .ToListAsync(cancellationToken);

            var mailVariants = new List<string>();
            var validVariants = new List<string>();

            var first = firstName.ToLower();
            var last = lastName.ToLower();
            var suffixEmail = "@info.uaic.ro";
            mailVariants.Add(first + "." + last + suffixEmail);
            mailVariants.Add(first + "." + last[0] + suffixEmail);
            mailVariants.Add(first[0] + "." + last + suffixEmail);
            mailVariants.Add(first[0..2] + "." + last + suffixEmail);
            mailVariants.Add(first + "." + last[0..2] + suffixEmail);
            mailVariants.Add(first + "." + last + '1' + suffixEmail);
            mailVariants.Add(first + "." + last[0] + '1' + suffixEmail);
            mailVariants.Add(first[0] + "." + last + '1' + suffixEmail);
            mailVariants.Add(first[0..2] + "." + last + '1' + suffixEmail);
            mailVariants.Add(first + "." + last[0..2] + '1' + suffixEmail);

            var counter = 0;

            while (counter < 3)
            {
                foreach (var mail in mailVariants)
                {
                    var ok = true;
                    foreach (var account in accounts)
                    {
                        if (mail == account.mail)
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

        public async Task<int> GenerateUidNumber(CancellationToken cancellationToken)
        {
            int uid;

            if (await dbContext.Accounts.AnyAsync(cancellationToken))
            {
                uid = await dbContext.Accounts.MaxAsync(x => x.uidNumber, cancellationToken);
                if (uid == 0 || uid == 7999)
                {
                    uid = 2000;
                }
                else
                {
                    uid = uid + 1;
                }
            }
            else
            {
                uid = 2000;
            }

            return uid;
        }

        public async Task<Result<MailInfoResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            var validationResult = new Query.Validator().Validate(request);
            if (!validationResult.IsValid)
            {
                var validResult = validationResult.ToString();
                if (validResult != null)
                {
                    return Result.Failure<MailInfoResponse>(new Error("GetMailInfo.ValidationFailed", validResult));
                }
            }

            // firstly search in our database
            var accountInstance = await dbContext.Accounts.FirstOrDefaultAsync(x => x.Matricol == request.Matricol, cancellationToken);

            if (accountInstance == null) //if it's not in our database
            {
                //if it does not exist in esims, return error
                //return Result.Failure<MailInfoResponse>(
                //                           new Error("GetMailInfo.Null", $"Account instance with Matricol {request.Matricol} not found."));

                //search in esims for this matricol
                //suppose we found the user in esims
                var uidNumber = await AddPartialEntryToDb(request.Matricol, cancellationToken);

                accountInstance = await dbContext.Accounts.FirstOrDefaultAsync(x => x.uidNumber == uidNumber.Value, cancellationToken);

                if (accountInstance == null)
                {
                    return Result.Failure<MailInfoResponse>(new Error("GetMailInfo.Null", $"User instance with ID {uidNumber} not found."));
                }

            }

            var mailVariants = await GenerateMailVariants(accountInstance.givenName, accountInstance.sn, cancellationToken);

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

public class GetMailInfoEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(Global.Version + "accounts/mail/{matricol}", async ([FromRoute] string matricol, ISender sender) =>
        {
            var query = new GetMailInfo.Query
            {
                Matricol = matricol
            };
            var result = await sender.Send(query);
            if (result.IsFailure)
            {
               if (result.Error.Code == "GetMailInfo.ValidationFailed")
                {
                    return Results.BadRequest(result.Error);
                }
                if (result.Error.Code == "GetMailInfo.Null")
                {
                    return Results.NotFound(result.Error);
                }
            }
            return Results.Ok(result.Value);
        }).WithTags("Accounts")
        .WithDescription("Endpoint for getting mail variants by user information. " +
        "If the request succeeds, in the response body you can find the mail addresses.")
        .Produces<MailInfoResponse>()
        .Produces<Error>(StatusCodes.Status404NotFound)
        .WithOpenApi();
    }
}
