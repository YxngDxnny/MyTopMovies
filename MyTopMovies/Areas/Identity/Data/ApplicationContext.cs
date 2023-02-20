using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyTopMovies.Areas.Identity.Data;
using MyTopMovies.Models;
using System.Reflection.Emit;
using System.Xml;

namespace MyTopMovies.Data;

public class ApplicationContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationContext(DbContextOptions<ApplicationContext> options)
        : base(options)
    {
    }
    public DbSet<TopMovieList> TopMovieLists { get; set; }
    public DbSet<MovieSelection> MovieSelections { get; set; }
    public DbSet<MovieChoice> MovieChoices { get; set; }
    public DbSet<FavouriteList> FavouriteLists { get; set; }
    public DbSet<FavouriteMovie> FavouriteMovies { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        RenameIdentityTables(builder);

        ConfigureModelRelationships(builder);
    }


    protected void RenameIdentityTables(ModelBuilder builder)
    {
        ///<summary>Renames the tables that store user identity</summary>
        ///<param name="builder">Model builder instance</param>
       
        base.OnModelCreating(builder);
        builder.HasDefaultSchema("UserMngt");
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable(name: "Users");
        });
        builder.Entity<IdentityRole>(entity =>
        {
            entity.ToTable(name: "Roles");
        });
        builder.Entity<IdentityUserRole<string>>(entity =>
        {
            entity.ToTable("UserRoles");
        });
        builder.Entity<IdentityUserClaim<string>>(entity =>
        {
            entity.ToTable("UserClaims");
        });
        builder.Entity<IdentityUserLogin<string>>(entity =>
        {
            entity.ToTable("UserLogins");
        });
        builder.Entity<IdentityRoleClaim<string>>(entity =>
        {
            entity.ToTable("RoleClaims");
        });
        builder.Entity<IdentityUserToken<string>>(entity =>
        {
            entity.ToTable("UserTokens");
        });
    }

    public void ConfigureModelRelationships(ModelBuilder builder)
    {
        builder.Entity<MovieSelection>()
        .HasMany(u => u.Choices)
        .WithOne(e => e.Selection)
        .HasForeignKey(x => x.SelectionID)
        .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<TopMovieList>()
       .HasMany(u => u.Favourites)
       .WithOne(e => e.MovieList)
       .HasForeignKey(x => x.ListID)
       .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<TopMovieList>()
        .HasMany(u => u.Selections)
        .WithOne(e => e.MovieList)
        .HasForeignKey(x => x.ListID)
        .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ApplicationUser>()
       .HasMany(u => u.Favourites)
       .WithOne(e => e.User)
       .HasForeignKey(x => x.UserID)
       .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ApplicationUser>()
        .HasMany(u => u.Selections)
        .WithOne(e => e.User)
        .HasForeignKey(x => x.UserID)
        .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ApplicationUser>()
       .HasMany(u => u.Lists)
       .WithOne(e => e.User)
       .HasForeignKey(x => x.UserID)
       .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ApplicationUser>()
       .HasMany(u => u.FavouriteMovies)
       .WithOne(e => e.User)
       .HasForeignKey(x => x.UserID)
       .OnDelete(DeleteBehavior.Restrict);
    }
    
}
