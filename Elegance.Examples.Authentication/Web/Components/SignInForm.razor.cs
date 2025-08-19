using System.Diagnostics;
using System.Threading.Tasks;
using Elegance.AspNet.Authentication;
using Elegance.Examples.Authentication.Models;
using Elegance.Examples.Authentication.Models.Requests;
using Elegance.Examples.Authentication.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;

namespace Elegance.Examples.Authentication.Web.Components
{
	public sealed partial class SignInForm : ComponentBase
	{
		private const string signInformName = nameof(SignInForm);
		private const string mfaFormName = "mfa";

		[CascadingParameter] public required HttpContext HttpContext { get; init; }

		[Inject] public required NavigationManager Navigation { get; init; }

		[Inject] public required AuthenticationService<User, DatabaseContext> Authentication { get; init; }

		[SupplyParameterFromForm(FormName = SignInForm.signInformName)]
		private SignInModel SignInModel { get; set; } = new();

		[SupplyParameterFromForm(FormName = SignInForm.mfaFormName)]
		private MfaModel MfaModel { get; set; } = new();

		private AuthenticationResult? authenticationStatus;

		private bool ShowMfaForm =>
			(this.authenticationStatus == AuthenticationResult.MfaRequired);

		private async Task SignInAsync()
		{
			Debug.Assert(this.SignInModel.IsValid);

			this.authenticationStatus = await this.Authentication.AuthenticateAsync(this.HttpContext,
																					this.SignInModel.Username,
																					this.SignInModel.Password,
																					this.SignInModel.Persistent);

			this.SignInModel.Password = null;

			if (this.authenticationStatus == AuthenticationResult.Success)
			{
				this.Navigation.NavigateTo("/account", true);
			}
		}

		private async Task MfaAsync()
		{
			Debug.Assert(this.MfaModel.IsValid);

			// You'd preferably keep track of the `persistent` state from the previous form model here.
			var success = await this.Authentication.AuthenticateTotpAsync(this.HttpContext, this.MfaModel.Totp, true);

			if (!success)
			{
				this.authenticationStatus = AuthenticationResult.MfaRequired;
				this.MfaModel.Totp = null;

				// @todo Show error
				return;
			}

			this.Navigation.NavigateTo("/account", true);
		}
	}
}
