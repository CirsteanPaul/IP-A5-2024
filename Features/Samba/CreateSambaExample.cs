using Carter;
using FluentValidation;
using IP.Project.Database;
using IP.Project.Features.Samba;
using IP.Project.Shared;
using MediatR;

namespace IP.Project.Features.Samba
{
    public static class CreateSambaExample
    {
        public record Command : IRequest<Result<int>> 
        {
            public int TestSamba { get; set; }
            
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.TestSamba).NotNull();
            }
        }

        public class Handler : IRequestHandler<Command, Result<int>>
        {
            private readonly ApplicationDBContext dbContext;
            private readonly IValidator<Command> validator;

            public Handler(ApplicationDBContext dbContext, IValidator<Command> validator)
            {
                this.dbContext = dbContext;
                this.validator = validator;
            }

            public async Task<Result<int>> Handle(Command request, CancellationToken cancellationToken)
            {
                var validationResult = validator.Validate(request);
                if (!validationResult.IsValid)
                {
                    return Result.Failure<int>(
                        new Error("CreateSamba.Validator", 
                        validationResult.ToString()!));
                }
                
                var samba = new Project.Entities.Samba
                {
                    TestSamba = 1,
                };

                // dbContext.Samba.Add(samba);
                await dbContext.SaveChangesAsync(cancellationToken);

                return samba.TestSamba;
            }
        }
    }
}

public class CreateSambaExampleEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        _ = app.MapPost("api/samba", async (int request, ISender sender) =>
        {
            var command = new CreateSambaExample.Command()
            {
                TestSamba = request
            };
            var result = await sender.Send(command);
            return result;
        });
    }
}
