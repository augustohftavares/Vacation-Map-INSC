using INSCmapaFerias.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace INSCmapaFerias.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Events> Events { get; set; }
    public DbSet<Paternity> Paternity { get; set; }
    public DbSet<Overtime> Overtime{ get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfiguration(new ApplicationUserEntityConfiguration());
        builder.ApplyConfiguration(new EventsConfiguration());

    }//end OnModelCreating();


}

// Configuração ApplicationUser
public class ApplicationUserEntityConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.FirstName).HasMaxLength(30);
        builder.Property(u => u.LastName).HasMaxLength(30);
        builder.Property(u => u.Email).HasMaxLength(89);
        builder.Property(u => u.PhoneNumber).HasMaxLength(12);
    }
}

//Configuração Events
public class EventsConfiguration : IEntityTypeConfiguration<Events>
{
    public void Configure(EntityTypeBuilder<Events> builder)
    {
        builder.Property(u => u.event_id).HasMaxLength(3);
        builder.Property(u => u.Title).HasMaxLength(100);
        builder.Property(u => u.event_start).HasMaxLength(9);
        builder.Property(u => u.event_end).HasMaxLength(9);
    }
}
