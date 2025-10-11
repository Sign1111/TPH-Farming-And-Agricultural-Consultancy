using Ade_Farming.Hubs;
using Ade_Farming.Models;
using Ade_Farming.Services;
using AspNetCore.Identity.MongoDbCore.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using System;
using System.Security.Claims; // 🟩 Added

var builder = WebApplication.CreateBuilder(args);

// ✅ MongoDB settings
var mongoSettings = new MongoDbSettings
{
    ConnectionString = "mongodb+srv://AdeFarming:Adeyinka2025@adefarming001.npnalnk.mongodb.net/?retryWrites=true&w=majority&appName=AdeFarming001",
    DatabaseName = "AdeFarmingDB"
};

var mongoClient = new MongoClient(mongoSettings.ConnectionString);

// ✅ Identity setup
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
})
.AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>(
    mongoSettings.ConnectionString,
    mongoSettings.DatabaseName
)
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// ✅ Seller policy
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SellerOnly", policy =>
        policy.RequireClaim("IsSeller", "True"));
});

// ✅ MongoDB DI
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = new MongoClient(mongoSettings.ConnectionString);
    return client.GetDatabase(mongoSettings.DatabaseName);
});

builder.Services.AddControllersWithViews();
builder.Services.AddTransient<IEmailSender, EmailSender>();

builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, NameUserIdProvider>();

// ✅ 🟩 Automatically re-issue "IsSeller" claim after login
builder.Services.Configure<IdentityOptions>(options => { });
builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, CustomUserClaimsPrincipalFactory>();

// ✅ Port setup for Render
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
builder.WebHost.UseUrls($"http://*:{port}");

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<AdminHub>("/adminHub");

// ✅ Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// ✅ Admin seeding
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    if (!await roleManager.RoleExistsAsync("Admin"))
        await roleManager.CreateAsync(new ApplicationRole("Admin"));

    var adminEmail = "admin@adefarming.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Super Admin"
        };
        await userManager.CreateAsync(adminUser, "Admin@123");
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}

app.Run();

// ✅ MongoDB Settings class
public class MongoDbSettings
{
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }
}

// 🟩 Add this class below Program.cs (keeps Seller claims persistent)
public class CustomUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>
{
    public CustomUserClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        Microsoft.Extensions.Options.IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor) { }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        // 🟩 Add Seller claim if true
        if (user.IsSeller)
        {
            identity.AddClaim(new Claim("IsSeller", "True"));
        }

        return identity;
    }
}
