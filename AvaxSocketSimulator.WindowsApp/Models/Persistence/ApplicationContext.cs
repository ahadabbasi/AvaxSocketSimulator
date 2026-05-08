using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using AvaxSocketSimulator.WindowsApp.Models.Domains;

namespace AvaxSocketSimulator.WindowsApp.Models.Persistence
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext() : base(DatabaseConnection.Instance().ConnectionString())
        {
            
        }

        public DbSet<Packet> Packets { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            EntityTypeConfiguration<Packet> packetBuilder = modelBuilder.Entity<Packet>();

            packetBuilder.HasKey(model => model.Id);

            packetBuilder.Property(model => model.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            packetBuilder.Property(model => model.Imei)
                .HasMaxLength(100);

            packetBuilder.Property(model => model.Type)
                .HasMaxLength(100);

            packetBuilder.Property(model => model.Data)
                .IsRequired();

            packetBuilder.Property(model => model.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
        }
    }
}
