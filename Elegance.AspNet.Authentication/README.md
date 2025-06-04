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

2. Create your authenticatable model (usually a user):

**User.cs**

```csharp
using Elegance.AspNet.Authentication;

public sealed class User : IAuthenticatable<User>
{
	[Column("id")] public int Id { get; init; }

	[Column("password", TypeName = "bytea")]
	public required byte[] Password { get; init; }
}
```

3. Create a `ClaimsProvider`:

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
		yield return new Claim("Id", user.Id.ToString());

		// Optionally, fetch data from a database or another service…
	}
}
```

4. Register the service on start-up:

**Program.cs**

```csharp
using Elegance.AspNet.Authentication.Extensions;

var builder = WebApplication.CreateBuilder(args);

// …

builder.Services.AddAuth();

// Optionally, you can add a callback as argument to configure the authentication cookie:

builder.Services.AddAuth(static (options) => 
{
	options.AccessDeniedPath = "/404";
	options.LoginPath = "/login";
	options.LogoutPath = "/logout";
});

var app = builder.Build();
```

5. Use the `AuthenticationService`:

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
		// Validate filled-in data from a form, for example…
		// Fetch the authenticating user's data…
        
		// You can verify the input password like this:
		if (!Hashing.Verify(userPasswordHash, inputPassword)) 
		{
			// Show error
		}

		// Additionally, when creating new users, you can hash a value like this:
		var bytes = Hashing.Hash("password123");

		var user = …;
		// Make this `true` to make the authentication cookie not expire.
		var persistent = false;

		await this.Authentication.LoginAsync(this.HttpContext, user, persistent);

		// Navigate to an account dashboard, or another protected route.
	}
    
	private Task LogoutAsync() =>
		this.Authentication.LogoutAsync(this.HttpContext);
}
```
