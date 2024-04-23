using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using IP.Project.Entities;

namespace IP.Project.Database
{
    public class VpnConfiguration : IEntityTypeConfiguration<VpnAccount>
    {
        public void Configure(EntityTypeBuilder<VpnAccount> builder)
        {
            builder.Property(x => x.IPv4Address).HasMaxLength(16);
        }
    }
}