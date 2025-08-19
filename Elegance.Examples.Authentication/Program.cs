using Elegance.AspNet.Authentication.Extensions;
using Elegance.Examples.Authentication.Models;
using Elegance.Examples.Authentication.Services;
using Elegance.Examples.Authentication.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents();

builder.Services.AddPooledDbContextFactory<DatabaseContext>((options) =>
{
	options.EnableDetailedErrors()
		   .EnableSensitiveDataLogging()
		   .EnableThreadSafetyChecks();

	options.UseSqlite(builder.Configuration.GetConnectionString("App"));
});

builder.Services.AddAuth<User, DatabaseContext, UserClaimsProvider>(
	configureCookie: static (options) =>
	{
		options.Cookie.Name = "Elegance.Auth";

		options.AccessDeniedPath = "/not-found";
		options.LoginPath = "/";
		options.LogoutPath = "/";
	},
	configureTotp: (options) =>
	{
		var totp = builder.Configuration.GetSection("Totp");

		options.Issuer = totp["Issuer"];
		options.SecretKey = totp["SecretKey"];
	}
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseHttpsRedirection();
}
else
{
	app.UseExceptionHandler("/error");
}

app.UseStatusCodePagesWithReExecute("/not-found");

app.MapStaticAssets();

app.UseAuth<User, DatabaseContext>();

app.UseAntiforgery();

app.MapRazorComponents<App>();

app.Run();
