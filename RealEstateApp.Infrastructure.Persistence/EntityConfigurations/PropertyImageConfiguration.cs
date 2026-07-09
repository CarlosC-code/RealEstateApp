using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Infrastructure.Persistence.Configurations
{
    public class PropertyImageConfiguration : IEntityTypeConfiguration<PropertyImage>
    {
        public void Configure(EntityTypeBuilder<PropertyImage> builder)
        {
            #region Basic configuration
            builder.HasKey(e => e.Id);
            builder.ToTable("PropertyImages");
            #endregion

            #region Property configurations
            builder.Property(e => e.ImageUrl).HasMaxLength(500);
            #endregion
        }
    }
}