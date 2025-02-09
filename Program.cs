#pragma warning disable ASP0014
using FunWebsiteThing;
using FunWebsiteThing.SQL;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using System.Buffers.Text;

Console.WriteLine($"GOOGLE_CLIENT_ID: {Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID")}");
Console.WriteLine($"GOOGLE_CLIENT_SECRET: {Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET")}");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddScoped<SessionManager>(); // adds SessionManager as a service
builder.Services.AddScoped<AccountController>(); // adds AccountController as a service
builder.Services.AddControllers(); // This adds all controllers to the servicecollection
builder.Services.AddHttpContextAccessor();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "TannerGhosensFunWebsiteThing.Session";
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.IdleTimeout = TimeSpan.FromHours(3); 
});
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme; // default auth scheme is cookie, which is what Sessions use
})
.AddCookie("Identity.External") // Register the external identity scheme
// A lot of the OAuth2.0 authentication is handled by middleware in ASP.NET Core, all we need to do is initiate a request to the provider and handle the response.
.AddGoogle(options => // add google oauth
{

    options.ClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID"); // retrieve client id from environment variable
    options.ClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET"); // retrieve client secret from environment variable
    options.CallbackPath = "/signin-google"; 
    // This is the callback path. ASP.NET Core middleware will handle the authentication process at this path, including
    // •	Validating the authentication token received from Google.
    // •	Creating a user principal (user identity) based on the claims (data given by a provider that creates an user's identity) provided by Google.
    // It is not /login or /login?method=google (the same page that initiates the login authorization) because the middleware doesn't like that and throws an irrecoverable oauth state error.
    options.SignInScheme = IdentityConstants.ExternalScheme; // we sign in with the external scheme as the default scheme is cookie otherwise, which is not what we want 
    options.Scope.Add("email");
    options.Scope.Add("profile");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers(); // this adds the endpoints of the controllers to the routes for this app
});

app.MapRazorPages();

Settings.Init();
FunWebsiteThing.SQL.Main.Init();

app.Run();