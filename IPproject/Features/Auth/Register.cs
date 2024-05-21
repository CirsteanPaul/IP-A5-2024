using Carter;
using FluentValidation;
using IP.Project.Contracts;
using IP.Project.Models;
using IP.Project.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using IP.Project.Database;
using IP.Project.Entities;
using IP.Project.Features.Auth;
using System;
using System.Data;

namespace IP.Project.Features.Auth
{
    public class RegisterRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class RegisterResponse
    {
        public string Username { get; init; }
        public string Email { get; init; }
    }

    public class Register
    {
        public record Command : IRequest<Result<RegisterResponse>>
        {
            public RegisterRequest Request { get; init; }
            public string Role { get; init; } = UserRoles.User; 
            public class Validator : AbstractValidator<Command>
            {
                public Validator()
                {
                    RuleFor(x => x.Request.Username).NotEmpty().MinimumLength(6);
                    RuleFor(x => x.Request.Email).NotEmpty().EmailAddress();
                    RuleFor(x => x.Request.Password).NotEmpty().MinimumLength(8)
                        .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                        .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                        .Matches("[0-9]").WithMessage("Password must contain at least one number.")
                        .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
                }
            }
        };

       

        public class Handler : IRequestHandler<Command, Result<RegisterResponse>>
        {
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly RoleManager<IdentityRole> _roleManager;

            public Handler(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
            {
                _userManager = userManager;
                _roleManager = roleManager;
            }

            public async Task<Result<RegisterResponse>> Handle(Command command, CancellationToken cancellationToken)
            {
                var validationResult = new Command.Validator().Validate(command);
                if (!validationResult.IsValid)
                {
                    return Result.Failure<RegisterResponse>(new Error("ValidationError", string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))));
                }

                var userExists = await _userManager.FindByNameAsync(command.Request.Username);
                if (userExists != null)
                    return Result.Failure<RegisterResponse>(new Error("UserAlreadyExists", "A user with this username already exists."));

                var emailExists = await _userManager.FindByEmailAsync(command.Request.Email);
                if (emailExists != null)
                    return Result.Failure<RegisterResponse>(new Error("EmailAlreadyExists", "A user with this email already exists."));

                var user = new ApplicationUser()
                {
                    UserName = command.Request.Username,
                    Email = command.Request.Email,
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                var createUserResult = await _userManager.CreateAsync(user, command.Request.Password);
                if (!createUserResult.Succeeded)
                {
                    return Result.Failure<RegisterResponse>(new Error("UserCreationFailed", createUserResult.Errors.First().Description));
                }

                if (!await _roleManager.RoleExistsAsync(command.Role))
                    await _roleManager.CreateAsync(new IdentityRole(command.Role));

                await _userManager.AddToRoleAsync(user, command.Role);

                return Result.Success(new RegisterResponse { Username = user.UserName, Email = user.Email });
            }
        }
    }

    public class RegisterEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            string apiVersion = Global.version;

            app.MapPost($"{apiVersion}auth/register", async (RegisterRequest request, ISender sender) =>
                {
                    var command = new Register.Command
                    {
                        Request = request
                    };
                var result = await sender.Send(command);

                if (result.IsSuccess)
                {
                    return Results.Created($"{apiVersion}auth/register/{result.Value.Username}", result.Value);
                }
                else
                {
                    if (result.Error.Code == "InternalServerError")
                    {
                        return Results.Json(new { Message = "An internal server error occurred." }, statusCode: 500);
                    }
                    else
                    {
                        return Results.BadRequest(new { Message = result.Error.Message });
                    }
                }
            })
            .Produces<RegisterResponse>(StatusCodes.Status201Created)
            .Produces<Error>(StatusCodes.Status400BadRequest)
            .Produces<Error>(StatusCodes.Status500InternalServerError)
            .WithTags("Auth")
            .WithDescription("Registers a new user and returns the user details on successful registration.")
            .WithOpenApi();
        }
    }
}