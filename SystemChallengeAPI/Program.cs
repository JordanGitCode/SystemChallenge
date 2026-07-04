using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using Scalar.AspNetCore;
using SystemChallengeAPI.Auth;
using SystemChallengeAPI.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(Policies.CanCapture, p => p.RequireRole(Roles.Capturer, Roles.Manager))
    .AddPolicy(Policies.CanApprove, p => p.RequireRole(Roles.Manager))
    .AddPolicy(Policies.CanSoftDelete, p => p.RequireRole(Roles.Manager));

builder.Services.AddScoped<IAuthorizationHandler, ApprovalHandler>();

builder.Services.AddSqlServer<ApplicationDbContext>
    (builder.Configuration.GetConnectionString("DefaultConnection"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
