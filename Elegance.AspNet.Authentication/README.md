# Elegance.AspNet.Authentication

Services, models and utilities used for base authentication in ASP.NET Core applications.

## Getting started

1. Install the package:

```xml

<Project>

	<ItemGroup>
		<PackageReference Include="Elegance.AspNet.Authentication" Version="0.3.0"/>
	</ItemGroup>

</Project>
```

2. Create your authenticatable database model (usually a user):

**User.cs**

```csharp
using Elegance.AspNet.Authentication;

public sealed class User : IAuthenticatable<User>
{
	[Column("id")] public int Id { get; init; }

	[Column("username")] public required string Username { get; init; }

	[Column("email")] public required string Email { get; init; }

	[Column("password", TypeName = "bytea")]
	public required byte[] Password { get; init; }

	[Column("security_stamp")]
	public string? SecurityStamp { get; }

	[Column("access_failed_count")]
	public int AccessFailedCount { get; }

	[Column("access_lockout_end")]
	public System.DateTimeOffset? AccessLockoutEnd { get; }

	public static abstract Expression<System.Func<TAuthenticatable, bool>> FindAuthenticatable(string user) =>
		static (u) => (u.Username == user) || (u.Email == user);
}
```

3. Create a `DbContext`:

**AppDbContext.cs**

```csharp
using Microsoft.EntityFrameworkCore;

public sealed class AppDbContext : DbContext
{
	public required DbSet<User> Users { get; init; }
}
```

4. Create a `ClaimsProvider`:

**UserClaimsProvider.cs**

```csharp
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using Elegance.AspNet.Authentication;

public sealed class UserClaimsProvider : IClaimsProvider<User>
{
	// Optionally, inject your dependency services here…
	public UserClaimsProvider()
	{
	}

	public async IAsyncEnumerable<Claim> GetClaimsAsync(User user, [EnumeratorCancellation] CancellationToken token)
	{ 
		yield return new Claim("Username", user.Username);

		// Optionally, fetch data from a database or another service…
	}
}
```

5. Register the service on start-up:

**Program.cs**

```csharp
using Elegance.AspNet.Authentication.Extensions;

var builder = WebApplication.CreateBuilder(args);

// …

builder.Services.AddAuth<User, AppDbContext, UserClaimsProvider>();

// Optionally, you can add a callback as argument to configure the authentication cookie:

builder.Services.AddAuth<User, AppDbContext, UserClaimsProvider>(static (options) => 
{
	options.AccessDeniedPath = "/404";
	options.LoginPath = "/login";
	options.LogoutPath = "/logout";
});

var app = builder.Build();

app.UseAuth<User, AppDbContext, UserClaimsProvider>();
```

6. Use the `AuthenticationService`:

**Login.razor.cs**

```csharp
using System.Threading.Tasks;
using Elegance.AspNet.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;

public sealed partial class Login : ComponentBase
{
	[CascadingParameter] public required HttpContext HttpContext { get; init; }
    
	[Inject] public required AuthenticationService<User> Authentication { get; init; }
    
	private async Task LoginAsync() 
	{
		var result = await this.Authentication.AuthenticateAsync(this.HttpContext, username, password, persistSession);

		// Result is one of the following;
		// AuthenticationResult.InvalidCredentials -> the user wasn't found, or the credentials don't match the ones in the database.
		// AuthenticationResult.TwoFactorRequired -> the user needs to fill in their 2FA code (not implemented yet).
		// AuthenticationResult.Success -> the user filled in valid credentials and is now signed in.

		// Navigate to an account dashboard, or another protected route.

		// It's also possible to manually sign in a user, while validating the data yourself:
		await this.Authentication.SignInAsync(this.HttpContext, user, persistSession);
	}
    
	private Task LogoutAsync() =>
		this.Authentication.LogoutAsync(this.HttpContext);
}
```

## Coming soon

- [ ] 2FA support
