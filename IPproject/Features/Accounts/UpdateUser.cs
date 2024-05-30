using Azure.Core;
using IP.Project.Contracts;
using Carter;
using FluentValidation;
using IP.Project.Database;
using IP.Project.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.DirectoryServices;
using System.Text.RegularExpressions;

namespace IP.Project.Features.Accounts;

public class UpdateUserInstance
{
    public record Command(int UidNumber, UpdateUserRequest Request) : IRequest<Result<int>>
    {
        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Request.Mail).EmailAddress().When(x => x.Request.Mail != null);
                RuleFor(x => x.Request.MailAlternateAddress).EmailAddress().When(x => x.Request.MailAlternateAddress != null);
            }
        }
    }

    public class Handler : IRequestHandler<Command, Result<int>>
    {
        private readonly ApplicationDBContext context;

        public Handler(ApplicationDBContext dbContext)
        {
            this.context = dbContext;
        }

        static private readonly string ldapServer = "LDAP://localhost:10389";
        static private readonly string adminUserName = "uid=admin,ou=system";
        static private readonly string adminPassword = "secret";

        public async Task<Result<int>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = new Command.Validator().Validate(request);
            var errorMessages = validationResult.Errors
            .Select(error => error.ErrorMessage)
            .ToList();
            if (!validationResult.IsValid)
            {
                return Result.Failure<int>(new Error("UpdateUser.ValidationFailed", string.Join(" ", errorMessages)));
            }

            // Update user in database
            var userInstance = await context.Accounts.FirstOrDefaultAsync(x => x.uidNumber == request.UidNumber, cancellationToken);

            if (userInstance == null)
            {
                return Result.Failure<int>(new Error("UpdateUser.Null", $"User instance with ID {request.UidNumber} not found."));
            }

            if (request.Request.Mail != null) 
            {   
                userInstance.mail = request.Request.Mail;
                userInstance.uid = request.Request.Mail.Split('@')[0];
                userInstance.homeDirectory = "/home/" + userInstance.uid;
            }

            if (request.Request.MailAlternateAddress != null) { userInstance.mailAlternateAddress = request.Request.MailAlternateAddress; }
            if (request.Request.UserPassword != null) { userInstance.userPassword = request.Request.UserPassword; }
            if (request.Request.TelephoneNumber != null) { userInstance.telephoneNumber = request.Request.TelephoneNumber; }

            userInstance.LastUpdatedOnUtc = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);



            // Update user in ldap server
            var directoryEntry = new System.DirectoryServices.DirectoryEntry(ldapServer, adminUserName, adminPassword, AuthenticationTypes.ServerBind);
            directoryEntry.Path = "LDAP://localhost:10389/dc=info,dc=uaic,dc=ro";

            var role = UidNumberToRole(userInstance.uidNumber);
            var fullOuPath = "";
            if (role == "students")
            {
                fullOuPath = OuToFullOuPath(userInstance.ou);
            }

            directoryEntry.Path = ldapServer + $"/ou={userInstance.ou} {fullOuPath},ou={role},dc=info, dc=uaic, dc=ro";

            try
            {
                // Create a new entry
                DirectorySearcher searcher = new DirectorySearcher(directoryEntry)
                {
                    PageSize = int.MaxValue,
                    Filter = $"(gidNumber={userInstance.gidNumber})"
                };

                // Find the user entry
                SearchResult result = searcher.FindOne();

                if (result != null)
                {
                    // Get the user entry
                    DirectoryEntry userEntry = result.GetDirectoryEntry();

                    // Set the properties
                    userEntry.Properties["cn"].Value = userInstance.cn;
                    userEntry.Properties["sn"].Value = userInstance.sn;
                    userEntry.Properties["gidNumber"].Value = userInstance.gidNumber;
                    userEntry.Properties["uidNumber"].Value = userInstance.uidNumber;
                    userEntry.Properties["uid"].Value = userInstance.uid;
                    userEntry.Properties["homeDirectory"].Value = userInstance.homeDirectory;
                    userEntry.Properties["displayName"].Value = userInstance.displayName;
                    userEntry.Properties["employeeNumber"].Value = userInstance.employeeNumber;
                    userEntry.Properties["givenName"].Value = userInstance.givenName;
                    userEntry.Properties["homePhone"].Value = userInstance.homePhone;
                    userEntry.Properties["initials"].Value = userInstance.initials;
                    userEntry.Properties["localityName"].Value = userInstance.localityName;
                    userEntry.Properties["mail"].Value = userInstance.mail;
                    userEntry.Properties["mobile"].Value = userInstance.mobile;
                    userEntry.Properties["ou"].Value = userInstance.ou;
                    userEntry.Properties["postalCode"].Value = userInstance.postalCode;
                    userEntry.Properties["roomNumber"].Value = userInstance.roomNumber;
                    userEntry.Properties["shadowInactive"].Value = userInstance.shadowInactive;
                    userEntry.Properties["street"].Value = userInstance.street;
                    userEntry.Properties["telephoneNumber"].Value = userInstance.telephoneNumber;
                    userEntry.Properties["title"].Value = userInstance.title;
                    userEntry.Properties["description"].Value = userInstance.description;

                    userEntry.Properties["userPassword"].Add(userInstance.userPassword);


                    userEntry.CommitChanges();
                    Console.WriteLine("User attributes updated successfully.");
                }
                else
                {
                    Console.WriteLine($"No user found with gidNumber={userInstance.uidNumber}");
                }

                Console.WriteLine("New LDAP entry created successfully.");
            }
            catch (Exception ex) 
            { 
                Console.WriteLine(ex.Message);
                return Result.Failure<int>(new Error("UpdateUser.Ldap", $"Failed to create user in ldap server"));
            }

            return Result.Success(request.UidNumber);
        }

        private static string UidNumberToRole(int uidNumber)
        {
            if (1000 <= uidNumber && uidNumber < 1200) {
                return "management";
            } else if (1200 <= uidNumber && uidNumber < 2000) {
                return "professors";
            } else if (2000 <= uidNumber && uidNumber < 8000) {
                return "students";
            } else {
                return "unknown";
            }
        }

        private static string OuToFullOuPath(string ou)
        {
            if (Regex.IsMatch(ou, @"^\d[A-Z]\d$"))
            {
                return ", ou=license";
            }
            else if (ou == "phd")
            {
                return "";
            }
            else
            {
                return ", ou=masters";
            }
        }
    }

}

public class UpdateUserEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut(Global.version + "accounts/{uidNumber:int}", async ([FromRoute] int uidNumber, [FromBody] UpdateUserRequest request, ISender sender) =>
        {
            var command = new UpdateUserInstance.Command(uidNumber, request);
            var result = await sender.Send(command);

            if (result.IsFailure)
            {
                return Results.NotFound(result.Error);
            }

            return Results.Ok(Global.version + $"accounts/{result.Value}");
        }).WithTags("Accounts")
        .WithDescription("Endpoint for creating an user by uidNumber updating with his chosen parameters " + "If the request succeeds, the updated account id will be returned.")
        .Produces<int>(StatusCodes.Status200OK) // TO DO CHECK 
        .Produces<Error>(StatusCodes.Status404NotFound)
        .WithOpenApi();
    }
}