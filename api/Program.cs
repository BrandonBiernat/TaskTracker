using System.Text;
using Dapper;
using DataAccessor;
using DataAccessor.TypeHandlers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Repositories;
using Services;
using Shared;
using Shared.Interfaces.DataManagement.DataAccessor;
using Shared.Interfaces.Repositories;
using Shared.Interfaces.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.SnakeCaseLower);
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? ["http://localhost:3000"])
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters {
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

builder.Services.AddSingleton<IDataAccessorOptions>(_ => 
    new DataAccessorOptions(
        builder.Configuration.GetConnectionString("Default")
        ?? throw new InvalidOperationException("Connection string not found")));

builder.Services.AddScoped<IDataAccessor, DataAccessor.DataAccessor>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

SqlMapper.AddTypeHandler(new StronglyTypedIdHandler<UserUID, Guid>(v => new UserUID(v), id => id.Value));

DefaultTypeMap.MatchNamesWithUnderscores = true;

WebApplication app = builder.Build();

app.UseExceptionHandler(error => error.Run(async context =>
{
    context.Response.StatusCode = 500;
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsJsonAsync(new { error = "An unexpected error occurred." });
}));

app.UseStatusCodePages(async context =>
{
    HttpResponse response = context.HttpContext.Response;
    response.ContentType = "application/json";

    string message = response.StatusCode switch
    {
        400 => "Bad request.",
        401 => "Unauthorized.",
        403 => "Forbidden.",
        404 => "Resource not found.",
        405 => "Method not allowed.",
        409 => "Conflict.",
        429 => "Too many requests.",
        _ => response.StatusCode >= 500
            ? "An unexpected error occurred."
            : "Request could not be processed."
    };

    await response.WriteAsJsonAsync(new { error = message });
});

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Run();