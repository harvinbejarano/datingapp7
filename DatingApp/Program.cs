using DatingApp;
using DatingApp.Data;
using DatingApp.Extensions;
using DatingApp.Interfaces;
using DatingApp.Middleware;
using DatingApp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
{
    // Add services to the container.
    builder.Services.AddControllers();
    builder.Services.AddApplicationServices(builder.Configuration);
    builder.Services.AddIdentityServices(builder.Configuration);
}

var app = builder.Build();
{
    // Configure the HTTP request pipeline.
    app.UseMiddleware<ExceptionMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseCors(corsBuilder => corsBuilder.AllowAnyHeader().AllowAnyMethod().WithOrigins("*") );
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<DataContext>();
        await context.Database.MigrateAsync();
        await Seed.SeedUsers(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetService<ILogger<Program>>();
        logger.LogError(ex, "AN error occured during migration");
    }

    app.Run();
}