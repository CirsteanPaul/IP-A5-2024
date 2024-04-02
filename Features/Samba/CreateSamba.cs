using Carter;
using FluentValidation;
using IP.VerticalSliceArchitecture.Contracts;
using IP.VerticalSliceArchitecture.Database;
using IP.VerticalSliceArchitecture.Features.Samba;
using IP.VerticalSliceArchitecture.Shared;
using Mapster;
using MediatR;

namespace IP.VerticalSliceArchitecture.Features.Samba
{
    public static class CreateSamba
    {
        public record Command : IRequest<Result<Guid>>, IRequest<Result<int>>
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
                    // Handle error
                    // return Result.Failure<Guid>(
                    //     new Error("CreateSamba.Validator", 
                    //     validationResult.ToString()));
                }
                
                var samba = new Entities.Samba
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

public class CreateSambaEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        _ = app.MapPost("api/samba", async (CreateArticleRequest request, ISender sender) =>
        {
            var command = request.Adapt<CreateSamba.Command>();
            var result = await sender.Send(command);
            return result;
        });
    }
}
