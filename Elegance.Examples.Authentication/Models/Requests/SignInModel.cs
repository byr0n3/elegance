using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Elegance.Examples.Authentication.Models.Requests
{
	internal sealed class SignInModel
	{
		[Required] public string? Username { get; set; }

		[Required] public string? Password { get; set; }

		public bool Persistent { get; set; }

		public bool IsValid
		{
			[MemberNotNullWhen(true, nameof(this.Username), nameof(this.Password))]
			get => (this.Username is not null) && (this.Password is not null);
		}
	}
}
