using Carter;
using IP.VerticalSliceArchitecture.Contracts;
using IP.VerticalSliceArchitecture.Database;
using IP.VerticalSliceArchitecture.Features.Articles;
using IP.VerticalSliceArchitecture.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IP.VerticalSliceArchitecture.Features.Articles
{
    public static class GetArticle
    {
        public class Query : IRequest<Result<ArticleResponse>>
        {
            public Guid Id { get; set; }
        }

        internal sealed class Handler : IRequestHandler<Query, Result<ArticleResponse>>
        {
            private readonly ApplicationDBContext context;

            public Handler(ApplicationDBContext context)
            {
                this.context = context;
            }
            public async Task<Result<ArticleResponse>> Handle(Query request, CancellationToken cancellationToken)
            {
                var article = await context.Articles
                    .AsNoTracking()
                    .Where(article => article.Id == request.Id)
                    .FirstOrDefaultAsync(cancellationToken);
                if (article is null) 
                {
                    return Result.Failure<ArticleResponse>(
                        new Error("GetArticle.Null", "Article not found"));
                }

                var articleResponse = new ArticleResponse
                {
                    Id = article.Id,
                    Title = article.Title,
                    Content = article.Content,
                    Tags = article.Tags.ToList(),
                    CreatedOnUtc = article.CreatedOnUtc,
                    PublishedOnUtc = article.PublishedOnUtc
                };
                return articleResponse;
            }
        }
    }
}

public class GetArticleEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/artcles/{id}", async(Guid id, ISender sender) =>
        {
            var query = new GetArticle.Query
            {
                Id = id
            };
            var result = await sender.Send(query);
            if (result.IsFailure)
            {
                return Results.NotFound(result.Error);
            }
            return Results.Ok(result.Value);
        });
    }
}
