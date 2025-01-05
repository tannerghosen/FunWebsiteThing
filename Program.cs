using FunWebsiteThing;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSingleton<SQLStuff>(); // This allows us to inject SQL operations into our files
builder.Services.AddSingleton<Misc>(); // This  allows us to use the controller MiscController without an error happening
builder.Services.AddScoped<SessionManager>();
builder.Services.AddControllers(); // This adds MiscController to the program
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
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute("api", "api/Misc/GeneratePassword"); // Adds the endpoint for GeneratePassword (api/Misc/GeneratePassword)
    endpoints.MapControllers();
    // Do note; basically endpoints follow this pattern: api/ClassName/MethodNameIfItIsPartOfTheController
});

app.UseAuthorization();

app.MapRazorPages();

SQLStuff _s = new SQLStuff(); // grab a copy of our SQLStuff
_s.Init(); // Create Database file before we start / add missing tables / important users
app.Run();