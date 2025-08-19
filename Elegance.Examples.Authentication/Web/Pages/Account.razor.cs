using System.Security.Claims;
using System.Threading.Tasks;
using Elegance.AspNet.Authentication;
using Elegance.Examples.Authentication.Models;
using Elegance.Examples.Authentication.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;

namespace Elegance.Examples.Authentication.Web.Pages
{
	public sealed partial class Account : ComponentBase
	{
		private const string signOutFormName = "sign-out";

		[CascadingParameter] public required HttpContext HttpContext { get; init; }

		[Inject] public required NavigationManager Navigation { get; init; }

		[Inject] public required AuthenticationService<User, DatabaseContext> Authentication { get; init; }

		private ClaimsPrincipal User =>
			this.HttpContext.User;

		private async Task SignOutAsync()
		{
			await this.Authentication.SignOutAsync(this.HttpContext);

			this.Navigation.NavigateTo("/", true);
		}
	}
}
