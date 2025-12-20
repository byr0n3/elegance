using Elegance.Examples.Authentication.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Elegance.Examples.Authentication.Services
{
	public sealed class DatabaseContext : DbContext
	{
		public required DbSet<User> Users { get; init; }

		public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.Entity<User>(static (u) =>
			{
				u.HasIndex(static (u) => u.Username)
				 .IsUnique();

				// The following property conversion registrations are only needed for SQLite;
				// SQLite doesn't have a built-in type for DateTimeOffset, and the EF-Core driver doesn't automagically handle conversion.

				u.Property(static (u) => u.LastSignInTimestamp)
				 .HasConversion(new DateTimeOffsetToBinaryConverter());

				u.Property(static (u) => u.AccessLockoutEnd)
				 .HasConversion(new DateTimeOffsetToBinaryConverter());
			});
		}
	}
}
