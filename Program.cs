#pragma warning disable ASP0014
using FunWebsiteThing;
using FunWebsiteThing.SQL;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;

Console.WriteLine($"GOOGLE_CLIENT_ID: {Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID")}");
Console.WriteLine($"GOOGLE_CLIENT_SECRET: {Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET")}");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddScoped<SessionManager>(); // adds sessionmanager as a service
builder.Services.AddScoped<AccountController>(); // Register AccountController as a service
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
.AddGoogle(options => // add google oauth as an authentication option
{

    options.ClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID"); // retrieve client id from environment variable
    options.ClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET"); // retrieve client secret from environment variable
    options.CallbackPath = "/signin-google"; // this is the redirect path after logging in
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