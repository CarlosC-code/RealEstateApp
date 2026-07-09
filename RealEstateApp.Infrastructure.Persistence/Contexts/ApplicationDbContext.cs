using Microsoft.EntityFrameworkCore;
using RealEstateApp.Core.Domain.Entities;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace RealEstateApp.Infrastructure.Persistence.Contexts
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Property> Properties { get; set; }
        public DbSet<PropertyType> PropertyTypes { get; set; }
        public DbSet<SaleType> SaleTypes { get; set; }
        public DbSet<Improvement> Improvements { get; set; }
        public DbSet<PropertyImage> PropertyImages { get; set; }
        public DbSet<PropertyImprovement> PropertyImprovements { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<FavoriteProperty> FavoriteProperties { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); //Liskov-substitution

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}