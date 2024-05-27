using IP.Project.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IP.Project.Database.Configurations
{
    public class VpnConfiguration : IEntityTypeConfiguration<VpnAccount>
    {
        public void Configure(EntityTypeBuilder<VpnAccount> builder)
        {
            builder.Property(x => x.IPv4Address).HasMaxLength(16);
        }
    }
}