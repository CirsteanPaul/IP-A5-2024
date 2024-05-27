using Carter;
using IP.Project.Contracts;
using IP.Project.Entities;
using IP.Project.Features.Auth;
using IP.Project.Resources;
using IP.Project.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;

namespace IP.Project.Features.Auth
{
    public record RegisterResponse
    {
        public string Username { get; init; }
        public string Email { get; init; }
    }

    public static class Register
    {
        public record Command : IRequest<Result<RegisterResponse>>
        {
            public RegisterRequest Request { get; init; }

            public class Validator : AbstractValidator<Command>
            {
                public Validator()
                {
                    RuleFor(x => x.Request.Username).NotEmpty().MinimumLength(6);
                    RuleFor(x => x.Request.Email).NotEmpty().EmailAddress();
                    RuleFor(x => x.Request.Password).NotEmpty().MinimumLength(6);
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


                    var userExists = await userManager.FindByNameAsync(request.Request.Username);
                    if (userExists != null)
                        return Result.Failure<RegisterResponse>(new Error("UserAlreadyExists", "A user with this username already exists."));

                    var emailExists = await userManager.FindByEmailAsync(request.Request.Email);
                    if (emailExists != null)
                        return Result.Failure<RegisterResponse>(new Error("EmailAlreadyExists", "A user with this email already exists."));
                    
                    ApplicationUser user = new ApplicationUser()
                    {
                        Email = request.Request.Email,
                        SecurityStamp = Guid.NewGuid().ToString(),
                        UserName = request.Request.Username
                    };

                    var createUserResult = await userManager.CreateAsync(user, request.Request.Password);
                    if (!createUserResult.Succeeded)
                    {
                        return Result.Failure<RegisterResponse>(new Error("UserCreationFailed", string.Join('\n', createUserResult.Errors.Select(x => x.Description))));
                    }

                    if (!await roleManager.RoleExistsAsync(UserRoles.User))
                    {
                        await roleManager.CreateAsync(new IdentityRole(UserRoles.User));
                    }

                    if (await roleManager.RoleExistsAsync(UserRoles.User))
                    {
                        await userManager.AddToRoleAsync(user, UserRoles.User);
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
}

public class RegisterEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost($"{Global.version}auth/register", async ([FromBody] RegisterRequest request, ISender sender) =>
        {
            var command = new Register.Command
            {
                Request = request
            };
            
            var result = await sender.Send(command);

            if (result.IsFailure)
            {
                if (result.Error.Code == "InternalServerError")
                {
                    return Results.StatusCode(500);
                }
                
                return Results.BadRequest(result.Error);
            }

            return Results.Created($"{Global.version}auth/register/{result.Value}", result.Value);

        })
            .WithTags("Auth")
            .WithDescription("Endpoint for registering a new user account." +
                                                                        "If the request is successful, it will return status code 201 (Created)). ")
            .Produces<RegisterResponse>(StatusCodes.Status201Created)
            .Produces<Error>(StatusCodes.Status500InternalServerError)
            .Produces<Error>(StatusCodes.Status400BadRequest)
            .WithOpenApi();
    }
}