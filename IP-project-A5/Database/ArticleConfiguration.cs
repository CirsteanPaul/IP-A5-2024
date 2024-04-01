using IP.VerticalSliceArchitecture.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

namespace IP.VerticalSliceArchitecture.Database
{
    public class ArticleConfiguration : IEntityTypeConfiguration<Article>
    {
        public void Configure(EntityTypeBuilder<Article> builder)
        {
            builder.Property(a => a.Tags).HasConversion
                (
                v => JsonConvert.SerializeObject(v, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                }),
                v => JsonConvert.DeserializeObject<List<string>>(v, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                })
                );
        }
    }
}
