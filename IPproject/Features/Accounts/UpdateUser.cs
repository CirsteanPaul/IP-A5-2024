using Azure.Core;
using IP.Project.Contracts;
using Carter;
using FluentValidation;
using IP.Project.Database;
using IP.Project.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using IP.Project.Entities;

namespace IP.Project.Features.Accounts
{
    public class UpdateUserInstance
    {
        public record Command(int uidNumber, UpdateUserRequest Request) : IRequest<Result<int>>
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

                var userInstance = await context.Accounts.FirstOrDefaultAsync(x => x.uidNumber == request.uidNumber, cancellationToken);

                if (userInstance == null)
                {
                    return Result.Failure<int>(new Error("UpdateUser.Null", $"User instance with ID {request.uidNumber} not found."));
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

                var ldapPath = "";
                var username = "";
                var password = "";

                using (DirectoryEntry entry = new DirectoryEntry(ldapPath, username, password))
                {
                    // Create a new entry
                    DirectoryEntry newUser = entry.Children.Add($"CN={account.cn}", "user");

                    // Set the properties
                    newUser.Properties["cn"].Value = account.cn;
                    newUser.Properties["sn"].Value = account.sn;
                    newUser.Properties["gidNumber"].Value = account.gidNumber;
                    newUser.Properties["uidNumber"].Value = account.uidNumber;
                    newUser.Properties["uid"].Value = account.uid;
                    newUser.Properties["homeDirectory"].Value = account.homeDirectory;
                    newUser.Properties["displayName"].Value = account.displayName;
                    newUser.Properties["employeeNumber"].Value = account.employeeNumber;
                    newUser.Properties["givenName"].Value = account.givenName;
                    newUser.Properties["homePhone"].Value = account.homePhone;
                    newUser.Properties["initials"].Value = account.initials;
                    newUser.Properties["localityName"].Value = account.localityName;
                    newUser.Properties["mail"].Value = account.mail;
                    newUser.Properties["mailAlternateAddress"].Value = account.mailAlternateAddress;
                    newUser.Properties["mobile"].Value = account.mobile;
                    newUser.Properties["ou"].Value = account.ou;
                    newUser.Properties["postalCode"].Value = account.postalCode;
                    newUser.Properties["roomNumber"].Value = account.roomNumber;
                    newUser.Properties["shadowInactive"].Value = account.shadowInactive;
                    newUser.Properties["street"].Value = account.street;
                    newUser.Properties["telephoneNumber"].Value = account.telephoneNumber;
                    newUser.Properties["title"].Value = account.title;
                    newUser.Properties["description"].Value = account.description;

                    // Set the password
                    newUser.Invoke("SetPassword", new object[] { account.userPassword });

                    // Commit the new entry
                    newUser.CommitChanges();

                    Console.WriteLine("New LDAP entry created successfully.");
                }

                return Result.Success(request.uidNumber);
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
}