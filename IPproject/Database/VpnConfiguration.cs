using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using IP.Project.Entities;

namespace IP.Project.Database
{
    public class VpnConfiguration : IEntityTypeConfiguration<Vpn>
    {
        public void Configure(EntityTypeBuilder<Vpn> builder)
        {
            builder.Property(x => x.IPv4Address).HasMaxLength(16);
        }
    }
}