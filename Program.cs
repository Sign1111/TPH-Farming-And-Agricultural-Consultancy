using Ade_Farming.Hubs;
using Ade_Farming.Models;
using Ade_Farming.Services;
using AspNetCore.Identity.MongoDbCore.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using System;

var builder = WebApplication.CreateBuilder(args);

// ✅ MongoDB settings
var mongoSettings = new MongoDbSettings
{
    ConnectionString = "mongodb+srv://AdeFarming:Adeyinka2025@adefarming001.npnalnk.mongodb.net/?retryWrites=true&w=majority&appName=AdeFarming001",
    DatabaseName = "AdeFarmingDB"
};

// ✅ Configure MongoClient explicitly with TLS 1.2
var clientSettings = MongoClientSettings.FromConnectionString(mongoSettings.ConnectionString);
clientSettings.SslSettings = new SslSettings
{
    EnabledSslProtocols = SslProtocols.Tls12
};
var mongoClient = new MongoClient(clientSettings)

// ✅ Proper Identity + Mongo setup (safe alternative to AddMongoIdentity)
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

// ✅ Configure authentication cookies
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// ✅ Authorization policy for Sellers
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SellerOnly", policy =>
        policy.RequireClaim("IsSeller", "True"));
});

// ✅ Register MongoDatabase instance for DI
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = new MongoClient(mongoSettings.ConnectionString);
    return client.GetDatabase(mongoSettings.DatabaseName);
});

builder.Services.AddControllersWithViews();
builder.Services.AddTransient<IEmailSender, EmailSender>();

builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, NameUserIdProvider>();

var app = builder.Build();

// ✅ Middleware pipeline
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

// ✅ Seed Admin role & user
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

public class MongoDbSettings
{
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }
}
