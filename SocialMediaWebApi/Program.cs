using Application.Auth.Account;
using Domain.Auth.Account;
using Infrastructure.Security;
using Infrastructure.Utility;
using Infrastructure.Utility.EmailUtility;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Persistence;
using System.Net;
using System.Net.Mail;

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
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login"; // Customize this if needed
    options.LogoutPath = "/Account/Logout"; // Customize this if needed
});

// Register the custom password hasher
builder.Services.AddSingleton<IPasswordHasher<UserModel>, CustomPasswordHasher<UserModel>>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000", "http://localhost:3001", "http://localhost:3002") // Adjust this URL to match your frontend URL
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Add authorization
builder.Services.AddAuthorization();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//  SMTP settings as a configuration

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
// Configure the SMTP client as a service
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddDistributedMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Use CORS middleware
app.UseCors("AllowSpecificOrigin");


app.MapControllers();

app.Run();
