using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Carter;
using IP.Project.Contracts;
using IP.Project.Database;
using IP.Project.Shared;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace IP.Project.Features.Samba
{
    public class GetAllSambas
    {
        public class Query : IRequest<Result<List<SambaResponse>>>;

        public class Handler : IRequestHandler<Query, Result<List<SambaResponse>>>
        {
            private readonly ApplicationDBContext context;

            public Handler(ApplicationDBContext context)
            {
                this.context = context;
            }

            public async Task<Result<List<SambaResponse>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var sambas = await context.SambaAccounts.AsNoTracking().ToListAsync(cancellationToken);

                if (sambas.Count == 0)
                {
                    return Result.Success(new List<SambaResponse>());
                }

                var sambaResponse = sambas.Adapt<List<SambaResponse>>();

                return Result.Success(sambaResponse);
            }
        }
    }

    public class GetAllSambasEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/v1/sambas", async (ISender sender) =>
            {
                var query = new GetAllSambas.Query();
                var result = await sender.Send(query);

                return Results.Ok(result.Value);
            }).WithTags("samba");
        }
    }
}