using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Elegance.AspNet.Authentication;
using Elegance.AspNet.Authentication.Extensions;
using Elegance.Examples.Authentication.Models;
using Elegance.Examples.Authentication.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QRCoder;

namespace Elegance.Examples.Authentication.Web.Components
{
	public sealed partial class ActivateTotp : ComponentBase
	{
		private const string formName = nameof(ActivateTotp);

		[CascadingParameter] public required HttpContext HttpContext { get; init; }

		[Parameter] [EditorRequired] public required ClaimsPrincipal User { get; set; }

		[Inject] public required TotpService Totp { get; init; }

		[Inject] public required NavigationManager Navigation { get; init; }

		[Inject] public required IDbContextFactory<DatabaseContext> DbFactory { get; init; }

		[Inject] public required AuthenticationService<User, DatabaseContext> Authentication { get; init; }

		[SupplyParameterFromForm(FormName = ActivateTotp.formName)]
		private TotpModel Model { get; set; } = new();

		private bool mfaEnabled;

		private string mfaUrl = string.Empty;
		private string mfaSvg = string.Empty;

		protected override void OnInitialized()
		{
			this.mfaEnabled = this.User.TryGetClaimValue<bool>("mfa", out var mfa) && mfa;

			if (!this.mfaEnabled)
			{
				this.GenerateMfaDetails();
			}
		}

		private void GenerateMfaDetails()
		{
			var username = this.User.GetRequiredClaimValue("username");

			this.mfaUrl = this.Totp.CreateUri(username);

			var qrGenerator = new QRCodeGenerator();
			var qrData = qrGenerator.CreateQrCode(this.mfaUrl, QRCodeGenerator.ECCLevel.Q, true);
			var svgQrCode = new SvgQRCode(qrData);

			this.mfaSvg = svgQrCode.GetGraphic(10);
		}

		private async Task ActivateAsync()
		{
			Debug.Assert(this.Model.Code is not null);

			var request = this.Totp.StartValidationRequest(this.Model.Code);

			var valid = this.Totp.VerifyTotpRequest(request);

			if (!valid)
			{
				this.Model.Code = null;
				return;
			}

			var user = await this.UpdateUserMfaAsync();

			// Sign in the user again. This allows the authentication cookie to contain valid, newly updated claims.
			// Manually setting the 'mfa' claim is faster, but this is easier to implement/read.
			await this.Authentication.SignInAsync(this.HttpContext, user, true);

			// Refresh the current page so the UI rerenders accordingly.
			this.Navigation.Refresh();
		}

		private async ValueTask<User> UpdateUserMfaAsync()
		{
			User user;

			var userId = this.User.GetClaimValue<int>("id");

			Debug.Assert(userId != default);

			var db = await this.DbFactory.CreateDbContextAsync();

			await using (db)
			{
				var updated = await db.Users
									  .Where((u) => u.Id == userId)
									  .ExecuteUpdateAsync(static (calls) => calls.SetProperty(static (u) => u.HasMfaEnabled, true));

				Debug.Assert(updated == 1);

				user = await db.Users.Where((u) => u.Id == userId).FirstAsync();
			}

			return user;
		}

		private sealed class TotpModel
		{
			[Required] public string? Code { get; set; }
		}
	}
}
