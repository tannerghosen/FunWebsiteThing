#pragma warning disable ASP0014
using FunWebsiteThing;
using FunWebsiteThing.SQL;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;


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
.AddCookie()
.AddGoogle(options => // add google oauth as an authentication option
{
    options.ClientId = "404091962983-67t5s4t8voodam372eomnodsiio1r8e4.apps.googleusercontent.com"; // client id
    options.ClientSecret = "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"; // client secret
    options.CallbackPath = "/login?method=google"; // this is the redirect path after loggin in which middleware (us) will handle the login by grabbing vital info
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