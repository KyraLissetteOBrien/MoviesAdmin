using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MoviesAdmin.Services;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// The order matters here to utilize the connection string with the variables
var connectionString = builder.Configuration.GetConnectionString("MoviesAdminContext") ?? throw new InvalidOperationException("Connection string 'MoviesAdminContext' not found."); 

builder.Services.AddDbContext<MoviesAdminContext>(options => options.UseSqlServer(connectionString));

// Start Auth0
builder.Services.AddAuth0WebAppAuthentication(options =>
{
    options.Domain = builder.Configuration["Auth0:Domain"];
    options.ClientId = builder.Configuration["Auth0:ClientId"];
    options.ClientSecret = builder.Configuration["Auth0:ClientSecret"];
    options.Scope = "openid profile email";
    options.OpenIdConnectEvents = new OpenIdConnectEvents
    {
        OnTokenValidated = async context =>
        {
            var principal = context.Principal!;

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = principal.Identity?.Name;

            if (string.IsNullOrEmpty(userId))
                return;

            var syncService = context.HttpContext.RequestServices
                .GetRequiredService<IUserSnapshotService>();

            await syncService.AddUserAsync(userId, email, name);
        }
    };
});

builder.Services.Configure<OpenIdConnectOptions>(Auth0Constants.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = "name",
        RoleClaimType = "https://localhost:7120/roles" // app's domain
    };
});
// End Auth0    

// Register new service
builder.Services.AddScoped<IUserSnapshotService, UserSnapshotService>();

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register Azure Blob service 
builder.Services.AddScoped<IAzureStorageService, AzureStorageService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Home/Status", "?code={0}");
app.UseHttpsRedirection();
app.UseStaticFiles(); //Auth0
app.UseRouting();
app.UseAuthentication(); //Auth0
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
