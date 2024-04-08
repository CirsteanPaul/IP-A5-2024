/*using Azure.Core;
using Carter;
using FluentValidation;
using IP.Project.Features.Vpn;
using IP.Project.Shared;
using MediatR;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Reflection;

namespace IP.Project.Features.Vpn
{
    public static class CreateVpn
    {
        public record Command : IRequest<Result<int>>
        {
            public string VPNName { get; set; }
            public string ServerAddress { get; set; }
            public int Port { get; set; }
            
        }
        public class CreateVPNValidator : AbstractValidator<CreateVPN.Command>
        {
            public CreateVPNValidator()
            {
                RuleFor(x = > x.VPNName).NotEmpty();
                RuleFor(x = > x.ServerAddress).NotEmpty().Matches();
                RuleFor(x = > x.Port).InclusiveBetween(1, 65535);
                                                                  
            }
        }

    }
    public class CreateVPNHandler : IRequestHandler<CreateVPN.Command, Result<int>>
    {
        private readonly VPNDbContext _dbContext;
        private readonly IValidator<CreateVPN.Command> _validator;

        public CreateVPNHandler(VPNDbContext dbContext, IValidator<CreateVPN.Command> validator)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task<Result<int>> Handle(CreateVPN.Command request, CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                return Result.Failure<int>(
                    new Error("CreateVPN.Validator",
                        validationResult.ToString()!));
            }

            var vpnInstance = new VPNInstance
            {
                Name = request.VPNName,
                ServerAddress = request.ServerAddress,
                Port = request.Port
            };

            _dbContext.VPNInstances.Add(vpnInstance);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return vpnInstance.Id;
        }
    }
}
public class CreateVPNEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/vpn", async(CreateVPN.Command request, ISender sender) = >
        {
            var result = await sender.Send(Request);
            return result;
        });
    }
}
*/