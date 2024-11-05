using Application.Auth.Account;
using Domain.Auth.Account;
using Infrastructure.Security;
using Infrastructure.Utility;
using Infrastructure.Utility.EmailUtility;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Persistence;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register the DbContext
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
                         b => b.MigrationsAssembly("Persistence")));

// Register MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Register.Handler).Assembly));

// Configure authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var key = jwtSettings["Secret"];

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"]
    };
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login"; // Customize this if needed
    options.LogoutPath = "/Account/Logout"; // Customize this if needed
});

// Register the custom password hasher
builder.Services.AddSingleton<IPasswordHasher<UserModel>, CustomPasswordHasher<UserModel>>();

builder.Services.Configure<Application.Account.validateJwtTokenBL.JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000", "http://localhost:3001", "http://localhost:3002")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Add authorization
builder.Services.AddAuthorization();

// Swagger/OpenAPI configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
// SMTP settings as a configuration
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddScoped<IJwtGenerator, JwtGenerator>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IUserAccessor, UserAccessor>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigin");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
