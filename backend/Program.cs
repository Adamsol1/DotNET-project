using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.OpenApi.Models;
using backend.Domain.Models;
using System.IO;
using backend.Application.Interfaces;
using backend.Application.Services.Authentication;
using backend.Application.Services.Game;
using backend.Application.Services.Story;
using backend.Infrastructure.Data;
using backend.Infrastructure.Repositories.Base;
using backend.Infrastructure.Repositories.Implementations;
using backend.Infrastructure.Logging;


// Clear default claim mappings to prevent issues with JWT token claims
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

// Create builder
var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));


///JSON web token
builder.Services.AddDbContext<AuthDbContext>(options =>
options.UseSqlite(builder.Configuration.GetConnectionString("AuthConnection")));
builder.Services.AddIdentity<AuthUser, IdentityRole>()
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();


//This code is based from the implementation in our course with coursecode. The connected github repository can be found here: ITPE3200, https://github.com/Baifan-Zhou/ITPE3200-25H/blob/main/6-React-Intro/Demo-react-9-authentication-backend/api/Program.cs
builder.Services.AddCors(options =>
    {
        options.AddPolicy("CorsPolicy", builder =>
            {
                builder.WithOrigins("http://localhost:3000", "http://localhost:5169")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
});

//This code is based from the implementation in our course with coursecode. The connected github repository can be found here: ITPE3200, https://github.com/Baifan-Zhou/ITPE3200-25H/blob/main/6-React-Intro/Demo-react-9-authentication-backend/api/Program.cs

// JWT Authentication configuration
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
        {
            // Configure JWT Bearer options
            options.SaveToken = true;
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                // Validate the JWT token parameters
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                    builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not configured")
                )),
                ClockSkew = TimeSpan.Zero  // This will remove the "grace" period of the JWT token. 
            };
        
        });


// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();


// DI registrations
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Specific repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICharacterRepository, CharacterRepository>();
builder.Services.AddScoped<IPlayerCharacterRepository, PlayerCharacterRepository>();
builder.Services.AddScoped<IStoryNodeRepository, StoryNodeRepository>();
builder.Services.AddScoped<IDialogueRepository, DialogueRepository>();
builder.Services.AddScoped<IChoiceRepository, ChoiceRepository>();

// Application services
builder.Services.AddScoped<IGenService, GenService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IStoryService, StoryService>();
builder.Services.AddScoped<IStoryControllerService, StoryControllerService>();
builder.Services.AddScoped<IGameService, GameService>();

// Logging service
builder.Services.AddScoped<IEntityFileLogger, EntityFileLogger>();


var app = builder.Build();


// Seed the database and apply migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var entityLogger = scope.ServiceProvider.GetRequiredService<IEntityFileLogger>();
    try
    {
        await DbSeeder.SeedAsync(dbContext);
        await entityLogger.LogAsync(
            "AppDbContext seeding completed successfully.",
            new { Timestamp = DateTime.UtcNow },
            LogCategories.System);
    }
    catch (Exception ex)
    {
        await entityLogger.LogAsync(
            "Error during AppDbContext seeding",
            new { Exception = ex.Message, StackTrace = ex.StackTrace },
            LogCategories.System);
        throw; // Re-throw the exception after logging it
    }
}

// Apply AuthDbContext migrations
using (var scope = app.Services.CreateScope())
{
    var authDbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    await authDbContext.Database.MigrateAsync();
}

// Seed auth database and apply migrations
using (var scope = app.Services.CreateScope())
{
    var authDbContext =
        scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    var entityLogger = scope.ServiceProvider.GetRequiredService<IEntityFileLogger>();
    try
    {
        await authDbContext.Database.MigrateAsync();
        await AuthDbSeeder.SeedAsync(
            authDbContext,
            scope.ServiceProvider,
            entityLogger
        );
        await entityLogger.LogAsync(
            "Auth database migration and seeding completed successfully.",
            new { Timestamp = DateTime.UtcNow },
            LogCategories.System);
    }
    catch (Exception ex)
    {
        await entityLogger.LogAsync(
            "Error during auth database migration/seeding",
            new { Exception = ex.Message, StackTrace = ex.StackTrace },
            LogCategories.System);
        throw;
    }
}





/// In production, use the standard exception handler and HSTS
app.UseExceptionHandler("/Home/Error");
app.UseHsts();
app.UseHttpsRedirection();



app.UseRouting();

app.UseCors("CorsPolicy");



//Authentication and authorization pipeline
app.UseAuthentication();
app.UseAuthorization();

/*
    * In production, the frontend and backend should be hosted on the same domain
    * and cors should be configured accordingly.
    * cors for the frontend to access the api
    * frontend hosted on localhost:3000
    * backend hosted on localhost:5169
    */




app.MapControllers();

app.MapGet("/", () => Results.Redirect("/Auth/Login"));
app.Run();
