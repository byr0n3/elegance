using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Elegance.AspNet.Authentication;
using Elegance.Examples.Authentication.Models;
using Elegance.Examples.Authentication.Models.Requests;
using Elegance.Examples.Authentication.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace Elegance.Examples.Authentication.Web.Components
{
	public sealed partial class SignUpForm : ComponentBase
	{
		private const string signUpFormName = nameof(SignUpForm);

		[Inject] public required IDbContextFactory<DatabaseContext> DbFactory { get; init; }

		[Inject] public required NavigationManager Navigation { get; init; }

		[SupplyParameterFromForm(FormName = SignUpForm.signUpFormName)]
		private SignUpModel SignUpModel { get; init; } = new();

		private async Task SignUpAsync()
		{
			Debug.Assert(this.SignUpModel.IsValid);

			var db = await this.DbFactory.CreateDbContextAsync();

			int saved;

			await using (db)
			{
				db.Users.Add(new User
				{
					Username = this.SignUpModel.Username,
					Password = Hashing.Hash(this.SignUpModel.Password),
					SecurityStamp = Guid.NewGuid().ToString(),
				});

				saved = await db.SaveChangesAsync();
			}

			Debug.Assert(saved == 1);

			this.Navigation.Refresh();
		}
	}
}
