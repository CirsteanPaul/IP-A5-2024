using Carter;
using IP.Project.Contracts;
using IP.Project.Database;
using IP.Project.Entities;
using IP.Project.Shared;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder; 
using Microsoft.AspNetCore.Routing;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IP.Project.Features.Vpn
{
    public class GetVpn
    {
        public class Query : IRequest<Result<VpnResponse>>
        {
            public Guid Id { get; set; }
        }

        internal sealed class Handler : IRequestHandler<Query, Result<VpnResponse>>
        {
            private readonly ApplicationDBContext context;

            public Handler(ApplicationDBContext context)
            {
                this.context = context;
            }

            public async Task<Result<VpnResponse>> Handle(Query request, CancellationToken cancellationToken)
            {
                var vpn = await context.Vpns.AsNoTracking().FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);

                if (vpn == null)
                {
                    return Result.Failure<VpnResponse>(
                        new Error("GetVpn.Null", "Vpn not found"));
                }

                var vpnResponse = vpn.Adapt<VpnResponse>();

                return Result.Success(vpnResponse);
            }
        }
    }

    public class GetVpnEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/vpn/{id}", async (Guid id, ISender sender) =>
            {
                var query = new GetVpn.Query
                {
                    Id = id
                };
                var result = await sender.Send(query);
                return result.IsSuccess ?
                    Results.Ok(result.Value) :
                    Results.NotFound(result.Error);
            });
        }
    }

    public class VpnResponse
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public string IPv4Address { get; set; }
    }
}
