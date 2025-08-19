using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Elegance.Examples.Authentication.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Elegance.Examples.Authentication.Models.Requests
{
	internal sealed class SignUpModel : IValidatableObject
	{
		[Required] public string? Username { get; set; }

		[Required] public string? Password { get; set; }

		public bool IsValid
		{
			[MemberNotNullWhen(true, nameof(this.Username), nameof(this.Password))]
			get => (this.Username is not null) && (this.Password is not null);
		}

		public IEnumerable<ValidationResult> Validate(ValidationContext context)
		{
			var dbFactory = context.GetRequiredService<IDbContextFactory<DatabaseContext>>();
			var db = dbFactory.CreateDbContext();

			using (db)
			{
				var usernameExists = db.Users.Any((u) => u.Username == this.Username);

				if (usernameExists)
				{
					yield return new ValidationResult("The entered username is already in use.", [nameof(this.Username)]);
				}
			}
		}
	}
}
