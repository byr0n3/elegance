using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using Elegance.AspNet.Authentication;

namespace Elegance.Examples.Authentication.Models
{
	[Table("users")]
	public sealed class User : IAuthenticatable<User>
	{
		[Column("id")] public int Id { get; init; }

		[Column("username")] public required string Username { get; init; }

		[Column("password")] public required byte[] Password { get; init; }

		[Column("security_stamp")] public required string? SecurityStamp { get; init; }

		[Column("last_sign_in_timestamp")] public System.DateTimeOffset? LastSignInTimestamp { get; set; }

		[Column("access_failed_count")] public int AccessFailedCount { get; init; }

		[Column("access_lockout_end")] public System.DateTimeOffset? AccessLockoutEnd { get; init; }

		[Column("mfa")] public bool HasMfaEnabled { get; init; }

		public static Expression<System.Func<User, bool>> FindAuthenticatable(string user, System.IServiceProvider _) =>
			(u) => u.Username == user;

		public static IQueryable<User> Include(IQueryable<User> queryable) =>
			queryable;
	}
}
