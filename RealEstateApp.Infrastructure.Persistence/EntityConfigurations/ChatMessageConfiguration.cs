using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Infrastructure.Persistence.Configurations
{
    public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
    {
        public void Configure(EntityTypeBuilder<ChatMessage> builder)
        {
            #region Basic configuration
            builder.HasKey(e => e.Id);
            builder.ToTable("ChatMessages");
            #endregion

            #region Property configurations
            builder.Property(e => e.Message).IsRequired();
            builder.Property(e => e.SenderId).IsRequired();
            builder.Property(e => e.ReceiverId).IsRequired();
            #endregion
        }
    }
}