﻿using Carter;
using FluentValidation;
using IP.Project.Entities;
using IP.Project.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IP.Project.Contracts.Auth;

namespace IP.Project.Features.Auth
{
    public static class Login {
        public record Command : IRequest<Result<LoginResponse>>
        {
            public LoginRequest Request { get; init; } = new();
            public class Validator : AbstractValidator<Command>
            {
                public Validator()
                {
                    RuleFor(x => x.Request.Username).NotEmpty();
                    RuleFor(x => x.Request.Password).NotEmpty().MinimumLength(6);
                }
            }
        }
        public class Handler : IRequestHandler<Command, Result<LoginResponse>>
        {
            private readonly UserManager<ApplicationUser> userManager;
            private readonly IConfiguration configuration;

            public Handler(UserManager<ApplicationUser> userManager, IConfiguration configuration)
            {
                this.userManager = userManager;
                this.configuration = configuration;
            }

            public async Task<Result<LoginResponse>> Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {
                    var validationResult = new Command.Validator().Validate(request);
                    if (!validationResult.IsValid)
                    {
                        var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
                        return Result.Failure<LoginResponse>(new Error("LoginValidationFailed", string.Join(" ", errorMessages)));
                    }

                    var user = await userManager.FindByNameAsync(request.Request.Username!);
                    if (user == null || !await userManager.CheckPasswordAsync(user, request.Request.Password!))
                    {
                        return Result.Failure<LoginResponse>(new Error("InvalidCredentials", "Invalid username or password."));
                    }

                    var authClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.UserName!),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    };
                    var userRoles = await userManager.GetRolesAsync(user);

                    authClaims.AddRange(userRoles.Select(userRole => new Claim(ClaimTypes.Role, userRole)));

                    var token = GenerateToken(authClaims);

                    return Result.Success(new LoginResponse { Token = token });
                }
                catch (Exception ex)
                {
                    return Result.Failure<LoginResponse>(new Error("InternalServerError", ex.Message));
                }
            }
            private string GenerateToken(IEnumerable<Claim> claims)
            {
                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]!));
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Issuer = configuration["JWT:ValidIssuer"]!,
                    Audience = configuration["JWT:ValidAudience"]!,
                    Expires = DateTime.UtcNow.AddHours(3),
                    SigningCredentials = new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256),
                    Subject = new ClaimsIdentity(claims)
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
        }
    }
    
    public class LoginEndPoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost($"{Global.Version}auth/login", async ([FromBody] LoginRequest request, ISender sender) =>
                {
                    var command = new Login.Command
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

                    return Results.Ok(result.Value);
                })
                .WithTags("Auth")
                .WithDescription("Endpoint for logging in a user. If the request is successful, it will return status code 200 (OK) with a JWT token.")
                .Produces<LoginResponse>()
                .Produces<Error>(StatusCodes.Status400BadRequest)
                .Produces<Error>(StatusCodes.Status500InternalServerError)
                .WithOpenApi();
        }
    }

}