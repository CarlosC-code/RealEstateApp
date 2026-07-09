using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Infrastructure.Persistence.Configurations
{
    public class PropertyConfiguration : IEntityTypeConfiguration<Property>
    {
        public void Configure(EntityTypeBuilder<Property> builder)
        {
            #region Basic configuration
            builder.HasKey(e => e.Id);
            builder.ToTable("Properties");
            #endregion

            #region Property configurations
            builder.Property(e => e.Code).IsRequired().HasMaxLength(6);
            builder.Property(e => e.Price).HasColumnType("decimal(18,2)");
            builder.Property(e => e.Description).IsRequired();
            builder.Property(e => e.AgentId).IsRequired();
            #endregion

            #region Relationships
            builder.HasMany(e => e.Images)
                .WithOne(e => e.Property)
                .HasForeignKey(e => e.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(e => e.PropertyImprovements)
                .WithOne(e => e.Property)
                .HasForeignKey(e => e.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(e => e.Offers)
                .WithOne(e => e.Property)
                .HasForeignKey(e => e.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(e => e.ChatMessages)
                .WithOne(e => e.Property)
                .HasForeignKey(e => e.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(e => e.FavoriteProperties)
                .WithOne(e => e.Property)
                .HasForeignKey(e => e.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}