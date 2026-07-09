using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Infrastructure.Persistence.Configurations
{
    public class PropertyImprovementConfiguration : IEntityTypeConfiguration<PropertyImprovement>
    {
        public void Configure(EntityTypeBuilder<PropertyImprovement> builder)
        {
            #region Basic configuration
            builder.HasKey(e => e.Id);
            builder.ToTable("PropertyImprovements");
            #endregion

            #region Relationships
            builder.HasOne(e => e.Improvement)
                .WithMany(e => e.PropertyImprovements)
                .HasForeignKey(e => e.ImprovementId)
                .OnDelete(DeleteBehavior.Restrict);
            #endregion
        }
    }
}