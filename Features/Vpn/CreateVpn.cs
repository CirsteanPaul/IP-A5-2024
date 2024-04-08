using FluentValidation;
using IP.Project.Database;
using IP.Project.Entities; 
using IP.Project.Shared;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IP.Project.Features.Vpn
{
    public static class CreateVpn
    {
        public record Command : IRequest<Result<Guid>>
        {
            public string? Description { get; set; }
            public string? IPv4Address { get; set; }
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Description).NotEmpty();
                RuleFor(x => x.IPv4Address).NotEmpty().Matches("^\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}$");
            }
        }

        public class Handler : IRequestHandler<Command, Result<Guid>>
        {
            private readonly ApplicationDBContext _dbContext;
            private readonly IValidator<Command> _validator;

            public Handler(ApplicationDBContext dbContext, IValidator<Command> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
            }

            public async Task<Result<Guid>> Handle(Command request, CancellationToken cancellationToken)
            {
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    return Result.Failure<Guid>(
                        new Error("CreateVpn.Validator",
                        validationResult.ToString()));
                }

                var vpn = new IP.Project.Entities.Vpn 
                {
                    Id = Guid.NewGuid(),
                    Description = request.Description,
                    IPv4Address = request.IPv4Address,
                };

                _dbContext.Vpns.Add(vpn);
                await _dbContext.SaveChangesAsync(cancellationToken);

                return vpn.Id;
            }
        }
    }
}
