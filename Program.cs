#pragma warning disable ASP0014
using FunWebsiteThing;
using FunWebsiteThing.SQL;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using System.Buffers.Text;

string sqlconstr = Environment.GetEnvironmentVariable("FWTConnectionString"); // FWTConnectionString, MySQL Connction String, syntax looks like this: Server=(server);Database=(db);User ID=(user);Password=(pass);
string gclientid = Environment.GetEnvironmentVariable("FWTGoogleClientId"); // FWTGoogleClientId, Google Client Id, used for OAuth 2.0 login
string gclientsec = Environment.GetEnvironmentVariable("FWTGoogleClientSecret"); // FWTGoogleClientSecret, Google Client Secret, used for OAuth 2.0 login
string domainname = Environment.GetEnvironmentVariable("FWTDomainName"); // FWTDomainName, Domain Name used for the website. (format: localhost or www.google.com)
/* To set up Google Login:
   1. Go to console.cloud.google.com
   2. Create an OAuth 2.0 Client ID
   3. Set an Authorized redirect URI to https://(website)/signin-google (if hosted locally, https://localhost:7081/signin-google)
   4. Save and wait roughly 5 minutes for it to take effect.
*/

bool[] setcheck = { sqlconstr != null, gclientid != null, gclientsec != null, domainname != null };
Logger.Write(domainname);
if (setcheck.Contains(false))
{
    // Log Fatal Error to FWT.log
    Logger.Write($"One or more of the environment variables is not set. You must add and set the environment variables listed in this error.", "ERROR");
    Logger.Write($"For more clarification, see the project's code in Program.cs", "ERROR");
    Logger.Write($"FWTConnectionString Set: {setcheck[0]} FWTGoogleClientId Set: {setcheck[1]} FWTGoogleClientSecret Set: {setcheck[2]} FWTDomainName Set: {setcheck[3]}","ERROR");

    // Display Fatal Error in console
    Console.WriteLine($"One or more of the environment variables is not set. You must add and set the environment variables listed in this error.");
    Console.WriteLine($"For more clarification, see the project's code in Program.cs");
    Console.WriteLine($"FWTConnectionString Set: {setcheck[0]} FWTGoogleClientId Set: {setcheck[1]} FWTGoogleClientSecret Set: {setcheck[2]} FWTDomainName Set: {setcheck[3]}");
    Console.ReadKey();

    Environment.Exit(0);
}

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "TannerGhosensFunWebsiteThing.Session";
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.IdleTimeout = TimeSpan.FromHours(3);
});
builder.Services.AddScoped<SessionManager>(); // adds SessionManager as a service
builder.Services.AddScoped<AccountController>(); // adds AccountController as a service
builder.Services.AddControllers(); // This adds all controllers to the servicecollection

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme; // default auth scheme is cookie, which is what Sessions use
})
.AddCookie("Identity.External") // Register the external identity scheme
// A lot of the OAuth2.0 authentication is handled by middleware in ASP.NET Core, all we need to do is initiate a request to the provider and handle the response.
.AddGoogle(options => // add google oauth
{

    options.ClientId = gclientid; // retrieve client id from environment variable
    options.ClientSecret = gclientsec; // retrieve client secret from environment variable
    options.CallbackPath = "/signin-google"; // do not change, /login or /login?method=google or similar would all be invalid, it has to be specific.
    // This is the callback path. ASP.NET Core middleware will handle the authentication process at this path, including
    // •	Validating the authentication token received from Google.
    // •	Creating a user principal (user identity) based on the claims (data given by a provider that creates an user's identity) provided by Google.
    options.SignInScheme = IdentityConstants.ExternalScheme; // we sign in with the external scheme as the default scheme is cookie otherwise, which is not what we want 
    options.Scope.Add("email");
    options.Scope.Add("profile");
});

var app = builder.Build();
app.UseCors("AllowAllOrigins");
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
FunWebsiteThing.SQL.Main.Init(sqlconstr);
FunWebsiteThing.WebSocketServer.Start(domainname); // Start the WebSocket Server
FunWebsiteThing.JavaScriptHelper.SetDomainName(domainname); // For any JavaScript function that needs the exact domain name
app.Run();