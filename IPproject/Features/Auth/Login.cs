using Carter;
using FluentValidation;
using IP.Project.Contracts;
using IP.Project.Contracts.Interfaces;
using IP.Project.Models;
using IP.Project.Models.Identity;
using IP.Project.Services;
using IP.Project.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IP.Project.Database;
using IP.Project.Entities;
using IP.Project.Features.Auth;
using System;
using System.Data;

namespace IP.Project.Features.Auth
{
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; init; } = string.Empty;
    }

    public class Login
    {
        public record Command(LoginRequest Request) : IRequest<Result<LoginResponse>>;

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Request.Username).NotEmpty().MinimumLength(6);
                RuleFor(x => x.Request.Password).NotEmpty().MinimumLength(8)
                    .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                    .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                    .Matches("[0-9]").WithMessage("Password must contain at least one number.")
                    .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
            }
        }

        public class Handler : IRequestHandler<Command, Result<LoginResponse>>
        {
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly IConfiguration _configuration;

            public Handler(UserManager<ApplicationUser> userManager, IConfiguration configuration)
            {
                _userManager = userManager;
                _configuration = configuration;
            }

            public async Task<Result<LoginResponse>> Handle(Command command, CancellationToken cancellationToken)
            {
                var validationResult = new Validator().Validate(command);
                if (!validationResult.IsValid)
                {
                    return Result.Failure<LoginResponse>(new Error("ValidationError", string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))));
                }

                var user = await _userManager.FindByNameAsync(command.Request.Username);
                if (user == null || !await _userManager.CheckPasswordAsync(user, command.Request.Password))
                {
                    return Result.Failure<LoginResponse>(new Error("InvalidCredentials", "Invalid username or password."));
                }

                var authClaims = new List<Claim>
                {new(ClaimTypes.Name, user.UserName ?? throw new InvalidOperationException("User name is null")),
                 new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                var token = GenerateToken(authClaims);

                return Result.Success(new LoginResponse { Token = token });
            }

            private string GenerateToken(IEnumerable<Claim> claims)
            {
                var secret = _configuration["JWT:Secret"];
                if (string.IsNullOrEmpty(secret))
                {
                    throw new InvalidOperationException("JWT secret is not configured properly.");
                }
                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Issuer = _configuration["JWT:ValidIssuer"],
                    Audience = _configuration["JWT:ValidAudience"],
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

    public class LoginEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            string apiVersion = Global.version;

            app.MapPost($"{apiVersion}auth/login", async (LoginRequest request, ISender sender) =>
            {
                var command = new Login.Command(request);
                var result = await sender.Send(command);

                if (result.IsSuccess)
                {
                    return Results.Ok(result.Value);
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
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .Produces<Error>(StatusCodes.Status400BadRequest)
            .Produces<Error>(StatusCodes.Status500InternalServerError)
            .WithTags("Authentication")
            .WithDescription("Logs in a user and returns a JWT token on successful authentication.")
            .WithOpenApi();
        }
    }
}
