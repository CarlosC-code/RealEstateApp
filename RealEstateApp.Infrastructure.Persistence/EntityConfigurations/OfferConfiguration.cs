using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Infrastructure.Persistence.Configurations
{
    public class OfferConfiguration : IEntityTypeConfiguration<Offer>
    {
        public void Configure(EntityTypeBuilder<Offer> builder)
        {
            #region Basic configuration
            builder.HasKey(e => e.Id);
            builder.ToTable("Offers");
            #endregion

            #region Property configurations
            builder.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            builder.Property(e => e.ClientId).IsRequired();
            #endregion
        }
    }
}