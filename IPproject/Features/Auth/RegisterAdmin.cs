using Carter;
using IP.Project.Entities;
using IP.Project.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using IP.Project.Constants;
using IP.Project.Contracts.Auth;

namespace IP.Project.Features.Auth
{
    public record RegisterAdminResponse
    {
        public string Username { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
    }

    public static class RegisterAdmin
    {
        public record Command : IRequest<Result<RegisterResponse>>
        {
            public Command(RegisterRequest request)
            {
                Request = request;
            }

            public RegisterRequest Request { get; init; }
            
            public class Validator : AbstractValidator<Command>
            {
                public Validator()
                {
                    RuleFor(x => x.Request.Email).NotEmpty().EmailAddress();
                }
            }
        }

        public class Handler : IRequestHandler<Command, Result<RegisterResponse>>
        {
            private readonly UserManager<ApplicationUser> userManager;
            private readonly RoleManager<IdentityRole> roleManager;

            public Handler(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
            {
                this.userManager = userManager;
                this.roleManager = roleManager; 
            }

            public async Task<Result<RegisterResponse>> Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {
                    var validationResult = new Command.Validator().Validate(request);
                    if (!validationResult.IsValid)
                    {
                        var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
                        return Result.Failure<RegisterResponse>(new Error("RegistrationValidationFailed", string.Join(" ", errorMessages)));
                    }
                    
                    var usernameValid = request.Request.Email!.Split("@").FirstOrDefault() == request.Request.Username;
                    if (!usernameValid)
                    {
                        return Result.Failure<RegisterResponse>(new Error("RegistrationUsernameFailed",
                            "Username should be the left part of the email."));
                    }

                    if (request.Request.Username != null)
                    {
                        var userExists = await userManager.FindByNameAsync(request.Request.Username);
                        if (userExists != null)
                            return Result.Failure<RegisterResponse>(new Error("UserAlreadyExists", "A user with this username already exists."));
                    }

                    var emailExists = await userManager.FindByEmailAsync(request.Request.Email);
                    if (emailExists != null)
                        return Result.Failure<RegisterResponse>(new Error("EmailAlreadyExists", "A user with this email already exists."));
                    
                    ApplicationUser user = new ApplicationUser()
                    {
                        Email = request.Request.Email,
                        SecurityStamp = Guid.NewGuid().ToString(),
                        UserName = request.Request.Username
                    };

                    if (request.Request.Password != null)
                    {
                        var createUserResult = await userManager.CreateAsync(user, request.Request.Password);
                        if (!createUserResult.Succeeded)
                        {
                            return Result.Failure<RegisterResponse>(new Error("UserCreationFailed", string.Join('\n', createUserResult.Errors.Select(x => x.Description))));
                        }
                    }

                    if (!await roleManager.RoleExistsAsync(Roles.Admin))
                    {
                        await roleManager.CreateAsync(new IdentityRole(Roles.Admin));
                    }

                    if (await roleManager.RoleExistsAsync(Roles.Admin))
                    {
                        await userManager.AddToRoleAsync(user, Roles.Admin);
                    }
                    
                    var response = new RegisterResponse
                    {
                        Username = user.UserName,
                        Email = user.Email
                    };

                    return Result.Success(response);
                }
                catch (Exception ex)
                {
                    return Result.Failure<RegisterResponse>(new Error("InternalServerError", ex.Message));
                }
            }
        }
    }
    
    public class RegisterAdminEndPoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost($"{Global.Version}auth/register-admin", async ([FromBody] RegisterRequest request, ISender sender) =>
                {
                    var command = new RegisterAdmin.Command(request);
            
                    var result = await sender.Send(command);

                    if (result.IsFailure)
                    {
                        if (result.Error.Code == "InternalServerError")
                        {
                            return Results.StatusCode(500);
                        }
                
                        return Results.BadRequest(result.Error);
                    }

                    return Results.Created($"{Global.Version}auth/register-admin/{result.Value}", result.Value);

                })
                .WithTags("Auth")
                .WithDescription("Endpoint for registering a new admin account." +
                                 "If the request is successful, it will return status code 201 (Created)). ")
                .Produces<RegisterResponse>(StatusCodes.Status201Created)
                .Produces<Error>(StatusCodes.Status500InternalServerError)
                .Produces<Error>(StatusCodes.Status400BadRequest)
                .WithOpenApi();
        }
    }
}