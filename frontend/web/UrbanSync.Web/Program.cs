using Microsoft.AspNetCore.Authentication.Cookies;
using UrbanSync.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.Cookie.Name = "UrbanSync.Web.Auth";
    });

builder.Services.AddHttpClient<IUrbanSyncApiClient, UrbanSyncApiClient>(client =>
{
    var baseUrl = builder.Configuration["UrbanSyncApi:BaseUrl"]
        ?? throw new InvalidOperationException("UrbanSyncApi:BaseUrl no esta configurado.");

    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddScoped<ActivityLogger>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
        ctx.Context.Response.Headers["X-Content-Type-Options"] = "nosniff"
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.MapGet("/health", () => Results.Ok(new
{
    application = "UrbanSync Web",
    status = "healthy",
    timestamp = DateTime.UtcNow
}));

app.Run();
