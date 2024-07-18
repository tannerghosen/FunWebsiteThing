using LearningASPNETAndRazor;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSingleton<Database>(); // This allows us to inject Database into our files
builder.Services.AddSingleton<Misc>(); // This (I believe?) allows us to use the controller MiscController without an error happening
builder.Services.AddControllers(); // This adds MiscController to the program
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
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
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute("api", "api/Misc/GeneratePassword"); // Adds the waypoint for GeneratePassword (api/Misc/GeneratePassword)
    endpoints.MapControllers();
    // Do note; basically waypoints follow this pattern: API/ClassName/MethodNameIfItIsPartOfTheController
});

app.UseAuthorization();

app.MapRazorPages();

app.Run();
