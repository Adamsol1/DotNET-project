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
using backend.Application;
using backend.Application.Dtos;
using backend.Infrastructure.Data;
using backend.Infrastructure.Repositories;
using backend.Application.Interfaces.Repositories;
using backend.Application.Interfaces.Services;

// Clear default claim mappings to prevent issues with JWT token claims
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                    builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not configured")
                ))
            };
            // NOTE  : the following code is for debuging given by Baifan. Remove on finished program
            options.Events = new JwtBearerEvents()
            {
                OnMessageReceived = context =>
                {
                    var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").LastOrDefault();
                    Console.WriteLine($"OnMessageReceived - Token: {token}");
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    Console.WriteLine("OnTokenValidated: SUCCESS");
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    Console.WriteLine($"OnAuthenticationFailed: {context.Exception.Message}");
                    Console.WriteLine($"Exception Type: {context.Exception.GetType().Name}");
                    if (context.Exception.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception: {context.Exception.InnerException.Message}");
                    }

                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    Console.WriteLine($"OnChallenge: {context.Error} - {context.ErrorDescription}");
                    return Task.CompletedTask;
                }
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



var loggerConfiguration = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File($"Logs/app_{DateTime.Now:yyyyMMdd_HHmmss}.log");

loggerConfiguration.Filter.ByExcluding(e => e.Properties.TryGetValue("SourceContext", out var value) &&
                            e.Level == LogEventLevel.Information &&
                            e.MessageTemplate.Text.Contains("Executed DbCommand"));

var logger = loggerConfiguration.CreateLogger();
builder.Logging.AddSerilog(logger);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.None);


var app = builder.Build();


// Seed the Game database and apply migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        await DbSeeder.SeedAsync(dbContext);
        logger.Information("Database migration and seeding completed successfully.");
    }
    catch (Exception ex)
    {
        logger.Error(ex, "An error occurred while migrating or seeding the database.");
        throw; // Re-throw the exception after logging it
    }
}
// Seed the Auth database and apply migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        await DbSeeder.SeedAsync(dbContext);
        logger.Information(
            "Database migration and seeding completed successfully."
        );
    }
    catch (Exception ex)
    {
        logger.Error(
            ex,
            "An error occurred while migrating or seeding the database."
        );
        throw;
    }
}

// Seed auth database and apply migrations
using (var scope = app.Services.CreateScope())
{
    var authDbContext =
        scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    try
    {
        await AuthDbSeeder.SeedAsync(
            authDbContext,
            scope.ServiceProvider,
            logger
        );
        logger.Information(
            "Auth database migration and seeding completed successfully."
        );
    }
    catch (Exception ex)
    {
        logger.Error(
            ex,
            "An error occurred while migrating or seeding the auth database."
        );
        throw;
    }
}




// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler("/Home/Error");
app.UseHsts();
app.UseHttpsRedirection();

/**
    * Enable static files to serve images from wwwroot folder
    * Images are stored in wwwroot/images folder
    * Example: https://localhost:5169/images/character1.png
    */
app.UseStaticFiles();
app.UseRouting();
// Debug middleware: log Origin, Method, Path and small request body for auth endpoints
app.Use(async (context, next) =>
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    var origin = context.Request.Headers["Origin"].FirstOrDefault() ?? "<no-origin>";
    logger.LogInformation("Incoming request: {Method} {Path} Origin:{Origin} Content-Type:{ContentType}",
        context.Request.Method, context.Request.Path, origin, context.Request.ContentType);

    // If this is an auth POST, read and log the small JSON body (enable buffering)
    if (context.Request.Path.StartsWithSegments("/api/auth") && context.Request.Method == HttpMethods.Post)
    {
        context.Request.EnableBuffering();
        using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;
        logger.LogDebug("Auth request body: {Body}", body);
    }

    await next();

    logger.LogInformation("Response for {Path} -> {StatusCode}", context.Request.Path, context.Response.StatusCode);
});
app.UseCors("CorsPolicy");

// NOTE THIS IS FOR DEBUG PURPOSE. It will be removed when authentication feature works. 
app.Use(async (context, next) =>
 {
     if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
     {
         var headerValue = authHeader.FirstOrDefault();
         if (headerValue?.StartsWith("Bearer ") == true)
         {
             var token = headerValue.Substring("Bearer ".Length).Trim();

             try
             {
                 // Decode the token to see its contents (without verification)
                 var handler = new JwtSecurityTokenHandler();
                 var jsonToken = handler.ReadJwtToken(token);

                 Console.WriteLine($"--> Token Issuer: {jsonToken.Issuer}");
                 Console.WriteLine($"--> Token Audience: {jsonToken.Audiences.FirstOrDefault()}");
                 Console.WriteLine($"--> Token Expiry: {jsonToken.ValidTo}");
                 Console.WriteLine($"--> Current Time: {DateTime.UtcNow}");
                 Console.WriteLine($"--> Config Issuer: {builder.Configuration["Jwt:Issuer"]}");
                 Console.WriteLine($"--> Config Audience: {builder.Configuration["Jwt:Audience"]}");
             }
             catch (Exception ex)
             {
                 Console.WriteLine($"--> Error decoding token: {ex.Message}");
             }
         }
     }
     await next.Invoke();
 });

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

//NOTE : This is commented out for testing with core made in the authorization. If sucesscfull this will be removed. 
/*
app.UseCors(cors => cors.WithOrigins("http://localhost:3000")
    .AllowAnyHeader()
    .AllowAnyMethod()
    );
*/
/*
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}"
);
*/

app.MapControllers();

app.MapGet("/", () => Results.Redirect("/Auth/Login"));
app.Run();
