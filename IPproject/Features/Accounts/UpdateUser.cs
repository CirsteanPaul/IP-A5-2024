using Carter;
using FluentValidation;
using IP.Project.Contracts;
using IP.Project.Database;
using IP.Project.Extensions;
using IP.Project.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Novell.Directory.Ldap;
using System.Text.RegularExpressions;

namespace IP.Project.Features.Accounts;

public partial class UpdateUserInstance
{
    public record Command(int UidNumber, UpdateUserRequest Request) : IRequest<Result<int>>
    {
        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.UidNumber).NotEmpty().UidNumber().When(x => x.UidNumber != 0);
                RuleFor(x => x.Request.TelephoneNumber).NotEmpty().OnlyNumbers().When(x => x.Request.TelephoneNumber != null);
                RuleFor(x => x.Request.Mail).UniversityEmailAddress().When(x => x.Request.Mail != null);
                RuleFor(x => x.Request.MailAlternateAddress).EmailAddress().When(x => x.Request.MailAlternateAddress != null);
            }
        }
    }

    public partial class Handler(ApplicationDBContext dbContext, ILdapService ldapService) : IRequestHandler<Command, Result<int>>
    {
        private readonly ApplicationDBContext context = dbContext;
        private readonly string ldapServer = ldapService.GetLdapSettings().LdapServer;
        private readonly int ldapPort = ldapService.GetLdapSettings().LdapPort;
        private readonly string adminUserName = ldapService.GetLdapSettings().AdminUserName;
        private readonly string adminPassword = ldapService.GetLdapSettings().AdminPassword;
        private readonly string baseDn = ldapService.GetLdapSettings().BaseDN;

        public async Task<Result<int>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = new Command.Validator().Validate(request);
            var errorMessages = validationResult.Errors
                .Select(error => error.ErrorMessage)
                .ToList();
            if (!validationResult.IsValid)
            {
                return Result.Failure<int>(new Error("UpdateUser.ValidationFailed", string.Join(", ", errorMessages)));
            }

            // Checks if there is another user with the same Mail or MailAlternateAddress
            var mailConflict = await context.Accounts.FirstOrDefaultAsync(x => x.mail == request.Request.Mail && x.uidNumber != request.UidNumber, cancellationToken);
            if (mailConflict != null)
            {
                return Result.Failure<int>(new Error("UpdateUser.DuplicateMail", "The specified mail is already used by another user."));
            }

            var mailAlternateAddressConflict = await context.Accounts.FirstOrDefaultAsync(x => x.mailAlternateAddress == request.Request.MailAlternateAddress && x.uidNumber != request.UidNumber, cancellationToken);
            if (mailAlternateAddressConflict != null)
            {
                return Result.Failure<int>(new Error("UpdateUser.DuplicateMailAlternateAddress", "The specified mail alternate address is already used by another user."));
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

            var role = UidNumberToRole(userInstance.uidNumber);
            var fullOuPath = role == "students" ? OuToFullOuPath(userInstance.ou) : "";
            var userDn = $"uid={userInstance.uid},ou={userInstance.ou},{fullOuPath},ou={role},{baseDn}";

            try
            {
                using var ldapConnection = new LdapConnection();
                // Connect and authenticate
                ldapConnection.Connect(ldapServer, ldapPort);
                ldapConnection.Bind(adminUserName, adminPassword);
                Console.WriteLine("LDAP: Authenticated");

                // Search for the user by gidNumber
                string searchFilter = $"(gidNumber={userInstance.gidNumber})";
                var searchResults = ldapConnection.Search(
                    baseDn,
                    LdapConnection.ScopeSub,
                    searchFilter,
                    null, // retrieve all attributes
                    false
                );

                if (searchResults.HasMore())
                {
                    var userEntry = searchResults.Next();
                    userDn = userEntry.Dn;

                    // Prepare modifications
                    var modifications = new[]
                    {
                        new LdapModification(LdapModification.Replace, new LdapAttribute("cn", userInstance.cn)),
                        new LdapModification(LdapModification.Replace, new LdapAttribute("sn", userInstance.sn)),
                        new LdapModification(LdapModification.Replace, new LdapAttribute("gidNumber", userInstance.gidNumber.ToString())),
                        new LdapModification(LdapModification.Replace, new LdapAttribute("uidNumber", userInstance.uidNumber.ToString())),
                        new LdapModification(LdapModification.Replace, new LdapAttribute("uid", userInstance.uid)),
                        new LdapModification(LdapModification.Replace, new LdapAttribute("homeDirectory", userInstance.homeDirectory)),
                        new LdapModification(LdapModification.Replace, new LdapAttribute("displayName", userInstance.displayName)),
                        new LdapModification(LdapModification.Replace, new LdapAttribute("employeeNumber", userInstance.employeeNumber)),
                        new LdapModification(LdapModification.Replace, new LdapAttribute("givenName", userInstance.givenName)),
                        new LdapModification(LdapModification.Replace, new LdapAttribute("homePhone", userInstance.homePhone)),
                        new LdapModification(LdapModification.Replace, new LdapAttribute("initials", userInstance.initials)),
                        new LdapModification(LdapModification.Replace, new LdapAttribute("localityName", userInstance.localityName)),
                        new LdapModification(LdapModification.Replace, new LdapAttribute("mail", userInstance.mail)),
                        new LdapModification(LdapModification.Replace, new LdapAttribute("mobile", userInstance.mobile)),
                        new LdapModification(LdapModification.Replace, new LdapAttribute("ou", userInstance.ou)),
                        new LdapModification(LdapModification.Replace, new LdapAttribute("postalCode", userInstance.postalCode)),
                        new LdapModification(LdapModification.Replace, new LdapAttribute("roomNumber", userInstance.roomNumber)),
                        new LdapModification(LdapModification.Replace, new LdapAttribute("shadowInactive", userInstance.shadowInactive)),
                        new LdapModification(LdapModification.Replace, new LdapAttribute("street", userInstance.street)),
                        new LdapModification(LdapModification.Replace, new LdapAttribute("telephoneNumber", userInstance.telephoneNumber)),
                        new LdapModification(LdapModification.Replace, new LdapAttribute("title", userInstance.title)),
                        new LdapModification(LdapModification.Replace, new LdapAttribute("description", userInstance.description)),
                        new LdapModification(LdapModification.Replace, new LdapAttribute("userPassword", userInstance.userPassword))
                    };

                    // Apply modifications
                    ldapConnection.Modify(userDn, modifications);
                    Console.WriteLine("LDAP: User attributes updated successfully.");
                }
                else
                {
                    Console.WriteLine($"LDAP: No user found with gidNumber={userInstance.gidNumber}");
                }
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

            return Result.Success(request.UidNumber);
        }

        private static string UidNumberToRole(int uidNumber)
        {
            if (1000 <= uidNumber && uidNumber < 1200)
            {
                return "management";
            }
            else if (1200 <= uidNumber && uidNumber < 2000)
            {
                return "professors";
            }
            else if (2000 <= uidNumber && uidNumber < 8000)
            {
                return "students";
            }
            else
            {
                return "unknown";
            }
        }

        private static string OuToFullOuPath(string ou)
        {
            if (GroupRegex().IsMatch(ou))
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

        [GeneratedRegex(@"^\d[A-Z]\d$")]
        private static partial Regex GroupRegex();
    }

}

public class UpdateUserEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut(Global.version + "accounts/mail/{uidNumber:int}", async ([FromRoute] int uidNumber, [FromBody] UpdateUserRequest request, ISender sender) =>
        {
            var command = new UpdateUserInstance.Command(uidNumber, request);
            var result = await sender.Send(command);

            if (result.IsFailure)
            {
                // if is failure due to duplicate mail return 409
                if (result.Error.Code == "UpdateUser.DuplicateMail")
                {
                    return Results.Conflict(result.Error.Message);
                }
                // if is failure due to duplicate mailAlternateAddress return 409
                if (result.Error.Code == "UpdateUser.DuplicateMailAlternateAddress")
                {
                    return Results.Conflict(result.Error.Message);
                }
                // if is failure due to invalid uidNumber return 409
                if (result.Error.Code == "UpdateUser.InvalidUidNumber")
                {
                    return Results.Conflict(result.Error.Message);
                }
                // if is failure due to incorrect email addresses return 400
                if (result.Error.Code == "UpdateUser.ValidationFailed")
                {
                    return Results.BadRequest(result.Error.Message);
                }
                // if is failure due to null user instance return 404
                if (result.Error.Code == "UpdateUser.Null")
                {
                    return Results.NotFound(result.Error.Message);
                }
            }

            return Results.Ok(Global.version + $"accounts/{result.Value}");
        }).WithTags("Accounts")
        .WithDescription("Endpoint for creating an user by uidNumber updating with his chosen parameters " + "If the request succeeds, the updated account id will be returned.")
        .Produces<int>(StatusCodes.Status200OK) 
        .Produces<Error>(StatusCodes.Status404NotFound)
        .WithOpenApi();
    }
}