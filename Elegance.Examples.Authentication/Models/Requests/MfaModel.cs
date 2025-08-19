using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Elegance.Examples.Authentication.Models.Requests
{
	internal sealed class MfaModel
	{
		[Required] public string? Totp { get; set; }

		public bool IsValid
		{
			[MemberNotNullWhen(true, nameof(this.Totp))]
			get => this.Totp is not null;
		}
	}
}
