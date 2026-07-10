using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ToDoApp.Domain.Entities;

namespace ToDoApp.Infrastructure.Configurations;

public class TagConfig : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(50);
        builder.Property(x => x.Name).IsRequired();

        builder.Property(x => x.ColorHex).HasMaxLength(10);
        builder.Property(x => x.ColorHex).IsRequired();

        builder.Property(x => x.BgColorHex).HasMaxLength(10);
        builder.Property(x => x.BgColorHex).IsRequired();

        builder.Property(x => x.UserId).IsRequired();
    }
}
