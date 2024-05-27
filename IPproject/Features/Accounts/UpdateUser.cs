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

            var role = UidNumberToRole(userInstance.uidNumber);
            var fullOuPath = "";
            if (role == "students")
            {
                fullOuPath = OuToFullOuPath(userInstance.ou);
            }

            var ouPath = ldapServer + $"/ou={userInstance.ou} {fullOuPath},ou={role},dc=info, dc=uaic, dc=ro";

            try
            {
                using DirectoryEntry entry = new(ouPath, adminUserName, adminPassword);
                // Create a new entry
                DirectoryEntry newUser = entry.Children.Add($"CN={userInstance.cn}", "user");

                // Set the properties
                newUser.Properties["cn"].Value = userInstance.cn;
                newUser.Properties["sn"].Value = userInstance.sn;
                newUser.Properties["gidNumber"].Value = userInstance.gidNumber;
                newUser.Properties["uidNumber"].Value = userInstance.uidNumber;
                newUser.Properties["uid"].Value = userInstance.uid;
                newUser.Properties["homeDirectory"].Value = userInstance.homeDirectory;
                newUser.Properties["displayName"].Value = userInstance.displayName;
                newUser.Properties["employeeNumber"].Value = userInstance.employeeNumber;
                newUser.Properties["givenName"].Value = userInstance.givenName;
                newUser.Properties["homePhone"].Value = userInstance.homePhone;
                newUser.Properties["initials"].Value = userInstance.initials;
                newUser.Properties["localityName"].Value = userInstance.localityName;
                newUser.Properties["mail"].Value = userInstance.mail;
                newUser.Properties["mailAlternateAddress"].Value = userInstance.mailAlternateAddress;
                newUser.Properties["mobile"].Value = userInstance.mobile;
                newUser.Properties["ou"].Value = userInstance.ou;
                newUser.Properties["postalCode"].Value = userInstance.postalCode;
                newUser.Properties["roomNumber"].Value = userInstance.roomNumber;
                newUser.Properties["shadowInactive"].Value = userInstance.shadowInactive;
                newUser.Properties["street"].Value = userInstance.street;
                newUser.Properties["telephoneNumber"].Value = userInstance.telephoneNumber;
                newUser.Properties["title"].Value = userInstance.title;
                newUser.Properties["description"].Value = userInstance.description;

                // Set the password
                newUser.Invoke("SetPassword", new object[] { userInstance.userPassword });

                // Commit the new entry
                newUser.CommitChanges();

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