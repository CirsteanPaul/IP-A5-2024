using Carter;
using FluentValidation;
using IP.Project.Contracts;
using IP.Project.Database;
using IP.Project.Entities;
using IP.Project.Features.Articles;
using IP.Project.Shared;
using Mapster;
using MediatR;

namespace IP.Project.Features.Articles
{
    public static class CreateArticle
    {
        public record Command : IRequest<Result<Guid>>
        {
            public string Title { get; set; } = string.Empty;
            public string Content { get; set; } = string.Empty;
            public List<string> Tags { get; set; } = new();
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Title).NotEmpty();
                RuleFor(x => x.Content).NotEmpty();
            }
        }

        public class Handler : IRequestHandler<Command, Result<Guid>>
        {
            private readonly ApplicationDBContext dbContext;
            private readonly IValidator<Command> validator;

            public Handler(ApplicationDBContext dbContext, IValidator<Command> validator)
            {
                this.dbContext = dbContext;
                this.validator = validator;
            }

            public async Task<Result<Guid>> Handle(Command request, CancellationToken cancellationToken)
            {
                var validationResult = validator.Validate(request);
                if (!validationResult.IsValid)
                {
                    return Result.Failure<Guid>(
                        new Error("CreateArticle.Validator", 
                        validationResult.ToString()!));
                }
                var article = new Article
                {
                    Id = Guid.NewGuid(),
                    Title = request.Title,
                    Content = request.Content,
                    Tags = request.Tags,
                    CreatedOnUtc = DateTime.UtcNow
                };

                dbContext.Articles.Add(article);
                await dbContext.SaveChangesAsync(cancellationToken);

                return article.Id;
            }
        }
    }
}

public class CreateArticleEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        _ = app.MapPost("api/articles", async (CreateArticleRequest request, ISender sender) =>
        {
            var command = request.Adapt<CreateArticle.Command>();
            var result = await sender.Send(command);
            if (result.IsFailure)
            {
                return Results.BadRequest(result.Error);
            }
            return Results.Ok($"/api/articles/{result.Value}");
        });
    }
}
