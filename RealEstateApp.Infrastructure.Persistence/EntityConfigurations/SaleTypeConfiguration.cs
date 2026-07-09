using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Infrastructure.Persistence.Configurations
{
    public class SaleTypeConfiguration : IEntityTypeConfiguration<SaleType>
    {
        public void Configure(EntityTypeBuilder<SaleType> builder)
        {
            #region Basic configuration
            builder.HasKey(e => e.Id);
            builder.ToTable("SaleTypes");
            #endregion

            #region Property configurations
            builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
            builder.Property(e => e.Description).IsRequired();
            #endregion
        }
    }
}