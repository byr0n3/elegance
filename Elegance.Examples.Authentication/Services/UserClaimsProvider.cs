using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading;
using Elegance.AspNet.Authentication;
using Elegance.Examples.Authentication.Models;

namespace Elegance.Examples.Authentication.Services
{
	internal sealed class UserClaimsProvider : IClaimsProvider<User>
	{
		public async IAsyncEnumerable<Claim> GetClaimsAsync(User user, [EnumeratorCancellation] CancellationToken _)
		{
			yield return new Claim("id", user.Id.ToString("N0", NumberFormatInfo.InvariantInfo));
			yield return new Claim("username", user.Username);
			yield return new Claim("mfa", user.HasMfaEnabled.ToString().ToLower());
		}
	}
}
