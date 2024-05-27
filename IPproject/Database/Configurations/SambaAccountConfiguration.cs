using IP.Project.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IP.Project.Database.Configurations;

public class SambaAccountConfiguration : IEntityTypeConfiguration<SambaAccount>
{
    public void Configure(EntityTypeBuilder<SambaAccount> builder)
    {
        builder.Property(x => x.IPv4Address).HasMaxLength(16);
    }
}