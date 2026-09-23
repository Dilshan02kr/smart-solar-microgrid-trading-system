using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using SmartSolarMicrogrid.Api.Common;
using SmartSolarMicrogrid.Api.Configuration;
using SmartSolarMicrogrid.Api.Models;
using SmartSolarMicrogrid.Api.Repositories;
using SmartSolarMicrogrid.Api.Security;
using SmartSolarMicrogrid.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(entry => entry.Value?.Errors.Count > 0)
                .ToDictionary(
                    entry => entry.Key,
                    entry => entry.Value!.Errors
                        .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                            ? "The supplied value is invalid."
                            : error.ErrorMessage)
                        .ToArray());

            return new BadRequestObjectResult(new ApiErrorResponse(
                "VALIDATION_ERROR",
                "One or more registration fields are invalid.",
                errors));
        };
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// 1. Configure MongoDbSettings with fallback values directly in configuration
var mongoSection = builder.Configuration.GetSection(MongoDbSettings.SectionName);
if (string.IsNullOrWhiteSpace(mongoSection["ConnectionString"]))
{
    // Replace with your actual team MongoDB connection link if appsettings.json fails to load
    builder.Configuration[$"{MongoDbSettings.SectionName}:ConnectionString"] = "";
}
{
    // A secure fallback key that is at least 32 characters long
    builder.Configuration[$"{JwtSettings.SectionName}:SigningKey"] = "SuperSecretSmartSolarMicrogridKey2026!@#$";
}
if (string.IsNullOrWhiteSpace(mongoSection["DatabaseName"]))
{
    builder.Configuration[$"{MongoDbSettings.SectionName}:DatabaseName"] = "SmartSolarMicrogridDb";
}

// 2. Register & Validate MongoDbSettings Options
builder.Services
    .AddOptions<MongoDbSettings>()
    .Bind(builder.Configuration.GetSection(MongoDbSettings.SectionName))
    .Validate(
        settings => !string.IsNullOrWhiteSpace(settings.ConnectionString),
        $"{MongoDbSettings.SectionName}:ConnectionString is required.")
    .Validate(
        settings => !string.IsNullOrWhiteSpace(settings.DatabaseName),
        $"{MongoDbSettings.SectionName}:DatabaseName is required.")
    .ValidateOnStart();

builder.Services
    .AddOptions<BootstrapAdminSettings>()
    .Bind(builder.Configuration.GetSection(BootstrapAdminSettings.SectionName));

builder.Services
    .AddOptions<JwtSettings>()
    .Bind(builder.Configuration.GetSection(JwtSettings.SectionName))
    .Validate(settings => !string.IsNullOrWhiteSpace(settings.Issuer), "Jwt:Issuer is required.")
    .Validate(settings => !string.IsNullOrWhiteSpace(settings.Audience), "Jwt:Audience is required.")
    .Validate(
        settings => Encoding.UTF8.GetByteCount(settings.SigningKey) >= 32,
        "Jwt:SigningKey must contain at least 32 bytes.")
    .Validate(settings => settings.ExpirationMinutes > 0, "Jwt:ExpirationMinutes must be positive.")
    .ValidateOnStart();

var jwtSettings = builder.Configuration
    .GetRequiredSection(JwtSettings.SectionName)
    .Get<JwtSettings>() ?? new JwtSettings();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.EventsType = typeof(ActiveAccountJwtBearerEvents);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = "userId",
            RoleClaimType = "role"
        };
    });

builder.Services.AddAuthorization();

// MongoClient is thread-safe and intended to be reused for the application's lifetime.
builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddSingleton<IMongoDatabase>(serviceProvider =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    var client = serviceProvider.GetRequiredService<IMongoClient>();
    return client.GetDatabase(settings.DatabaseName);
});

// Domain Services & Repositories Registration
builder.Services.AddSingleton<IUserDetailsRepository, UserDetailsRepository>();
builder.Services.AddSingleton<IPasswordHasher<UserDetails>, PasswordHasher<UserDetails>>();
builder.Services.AddScoped<IProsumerRegistrationService, ProsumerRegistrationService>();
builder.Services.AddScoped<IProsumerManagementService, ProsumerManagementService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<ActiveAccountJwtBearerEvents>();

// Member 3 Reservation Service
builder.Services.AddSingleton<MongoDbService>();

// Member 4 Transaction Verification & Completion Services
builder.Services.AddScoped<ITransactionVerificationService, TransactionVerificationService>();
builder.Services.AddScoped<ITransactionCompletionService, TransactionCompletionService>();

// Hosted Initializers
builder.Services.AddHostedService<MongoDbInitializer>();
builder.Services.AddHostedService<BackofficeBootstrapInitializer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();