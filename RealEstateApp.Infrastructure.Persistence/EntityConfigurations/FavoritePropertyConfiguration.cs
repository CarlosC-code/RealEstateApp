using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Infrastructure.Persistence.Configurations
{
    public class FavoritePropertyConfiguration : IEntityTypeConfiguration<FavoriteProperty>
    {
        public void Configure(EntityTypeBuilder<FavoriteProperty> builder)
        {
            #region Basic configuration
            builder.HasKey(e => e.Id);
            builder.ToTable("FavoriteProperties");
            #endregion

            #region Property configurations
            builder.Property(e => e.ClientId).IsRequired();
            #endregion
        }
    }
}