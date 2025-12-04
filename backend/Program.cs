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

//Hentet fra pensum : https://github.com/Baifan-Zhou/ITPE3200-25H/blob/main/6-React-Intro/Demo-react-9-authentication-backend/api/Program.cs
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

// Hentet fra pensum for debug. kan fjerne etterhvert. 
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My Shop API", Version = "v1" }); // Basic info for the API
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme // Define the Bearer auth scheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement // Require Bearer token for accessing the API
    {{ new OpenApiSecurityScheme // Reference the defined scheme
        { Reference = new OpenApiReference
        { Type = ReferenceType.SecurityScheme,
            Id = "Bearer"}},
        new string[] {}
    }});
});

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
                ClockSkew = TimeSpan.Zero  // ◄─── Remove default 5-minute grace period
            };
        
        });





// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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




// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

/// In production, use the standard exception handler and HSTS
app.UseExceptionHandler("/Home/Error");
app.UseHsts();
app.UseHttpsRedirection();



app.UseRouting();
// Debug middleware: log Origin, Method, Path and small request body for auth endpoints
app.Use(async (context, next) =>
{
    var entityLogger = context.RequestServices.GetRequiredService<IEntityFileLogger>();
    var origin = context.Request.Headers["Origin"].FirstOrDefault() ?? "<no-origin>";
    await entityLogger.LogAsync(
        "Incoming request",
        new { Method = context.Request.Method, Path = context.Request.Path.Value, Origin = origin, ContentType = context.Request.ContentType },
        LogCategories.System);

    // If this is an auth POST, read and log the small JSON body (enable buffering)
    if (context.Request.Path.StartsWithSegments("/api/auth") && context.Request.Method == HttpMethods.Post)
    {
        context.Request.EnableBuffering();
        using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;
        await entityLogger.LogAsync("Auth request body", new { Body = body }, LogCategories.System);
    }

    await next();

    await entityLogger.LogAsync(
        "Response sent",
        new { Path = context.Request.Path.Value, StatusCode = context.Response.StatusCode },
        LogCategories.System);
});
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
