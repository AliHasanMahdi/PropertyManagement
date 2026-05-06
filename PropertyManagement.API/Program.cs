using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PropertyManagement.API.Data;
using System.Text;
using System.Reflection;
using Microsoft.Extensions.Logging;
using PropertyManagement.API.Services;
using PropertyManagement.API.Services.Interfaces;


var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// JWT Authentication
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
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

// Register Services
builder.Services.AddScoped<IBuildingService, BuildingService>();
builder.Services.AddScoped<IMaintenanceService, MaintenanceService>();
builder.Services.AddScoped<ILeaseService, LeaseService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();



app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Diagnostic wrapper: catch ReflectionTypeLoadException and log LoaderExceptions + assemblies that fail GetTypes()
try
{
    app.MapControllers();
}
catch (ReflectionTypeLoadException ex)
{
    var logger = app.Logger;

    logger.LogError("ReflectionTypeLoadException while mapping controllers: {Message}", ex.Message);

    if (ex.LoaderExceptions != null)
    {
        foreach (var le in ex.LoaderExceptions)
        {
            if (le == null) continue;
            logger.LogError("LoaderException: {Type} - {Message}", le.GetType().FullName, le.Message);
            logger.LogError(le.StackTrace);
        }
    }

    // Extra diagnostics: find assemblies that throw when enumerating types
    foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
    {
        try
        {
            // Force type enumeration to catch TypeLoadException / BadImageFormatException thrown by problematic assemblies
            var types = asm.GetTypes();
        }
        catch (Exception aex)
        {
            logger.LogError("Assembly '{Name}' threw while GetTypes(): {Type} - {Message}", asm.FullName, aex.GetType().FullName, aex.Message);
            logger.LogError(aex.StackTrace);
        }
    }

    // Re-throw so the process still fails (unless you want to continue)
    throw;
}

if (app.Environment.IsDevelopment())
 {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();