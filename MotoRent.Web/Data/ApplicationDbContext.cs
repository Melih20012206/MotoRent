using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MotoRent.Web.Models;

namespace MotoRent.Web.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public const string DateOfBirthProperty = "DateOfBirth";

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<RentalListing> RentalListings => Set<RentalListing>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<IdentityUser>()
            .Property<DateOnly?>(DateOfBirthProperty)
            .HasColumnType("date")
            .IsConcurrencyToken();

        builder.Entity<RentalListing>(listing =>
        {
            listing.Property(x => x.DailyPrice)
                .HasPrecision(10, 2);

            listing.HasOne(x => x.Owner)
                .WithMany()
                .HasForeignKey(x => x.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            listing.ToTable(table => table.HasCheckConstraint(
                "CK_RentalListings_DailyPrice",
                "[DailyPrice] BETWEEN 1 AND 10000"));
        });
    }
}