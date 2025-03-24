using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Synaplic.BlazorJwtApp.Server.Authentication;
using Synaplic.BlazorJwtApp.Shared;
using Synaplic.BlazorJwtApp.Shared.Permissions;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Ajout des services

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});


builder.Services.AddAuthorization(options =>
{
    // 🔹 Automatically create a policy for each permission
    foreach (var permission in Permissions.All)
    {
        options.AddPolicy(permission, policy => policy.RequireClaim(AuthenticationConstants.AuthPermission, permission));
    }
});


builder.Services.AddScoped<TokenService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<ApplicationDbContext>();

    try
    {
        Console.WriteLine("📌 Applying database migrations...");
        await dbContext.Database.MigrateAsync();
        Console.WriteLine("✅ Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error applying database migrations: {ex.Message}");
    }
}


// 🔹 Exécution du seeding des utilisateurs et rôles
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    try
    {
        Console.WriteLine("📌 Starting user and role seeding...");
        await SeedUsersAndRoles(userManager, roleManager);
        Console.WriteLine("✅ User and role seeding completed.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error while seeding users and roles: {ex.Message}");
    }
}

// 🔹 Configuration du pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();



// -------------------- 📌 SEED FUNCTION --------------------
async Task SeedUsersAndRoles(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
{
    Console.WriteLine("🔹 Seeding users, roles, and permissions...");

    // 🔹 Define roles and their claims
    var rolesWithClaims = new Dictionary<string, List<string>>
    {
        { "Admin", Permissions.All },
        { "Basic", Permissions.Basics }
    };

    // 🔹 Create roles and assign claims
    foreach (var role in rolesWithClaims)
    {
        await CreateRoleWithClaims(roleManager, role.Key, role.Value);
    }

    // 🔹 Create users and assign roles
    await CreateUserWithRole(userManager, "admin@synaplic.com", "admin", "Admin");
    await CreateUserWithRole(userManager, "user@synaplic.com", "user", "Basic");
}

// 🔹 Function to create a role and add claims if missing
async Task CreateRoleWithClaims(RoleManager<IdentityRole> roleManager, string roleName, List<string> claims)
{
    var roleExists = await roleManager.RoleExistsAsync(roleName);
    if (!roleExists)
    {
        Console.WriteLine($"📌 Creating role: {roleName}");
        await roleManager.CreateAsync(new IdentityRole(roleName));
    }

    var role = await roleManager.FindByNameAsync(roleName);
    if (role != null)
    {
        var existingClaims = await roleManager.GetClaimsAsync(role);
        foreach (var claim in claims)
        {
            if (!existingClaims.Any(c => c.Type == AuthenticationConstants.AuthPermission && c.Value == claim))
            {
                await roleManager.AddClaimAsync(role, new Claim(AuthenticationConstants.AuthPermission, claim));
                Console.WriteLine($"✅ Added claim {claim} to {roleName} role.");
            }
        }
    }
}

// 🔹 Function to create a user and assign a role if missing
async Task CreateUserWithRole(UserManager<IdentityUser> userManager, string email, string username, string role)
{
    var user = await userManager.FindByEmailAsync(email);
    if (user == null)
    {
        Console.WriteLine($"📌 Creating {role} user...");
        user = new IdentityUser { UserName = username, Email = email, EmailConfirmed = true };

        var result = await userManager.CreateAsync(user, "Password+25");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, role);
            Console.WriteLine($"✅ {role} user created.");
        }
        else
        {
            Console.WriteLine($"❌ Error creating {role} user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
    else
    {
        Console.WriteLine($"✅ {role} user already exists.");
    }
}
