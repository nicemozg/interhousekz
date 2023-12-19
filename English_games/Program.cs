// using System;
// using System.Globalization;
// using System.Text.Json.Serialization;
// using English_games.Context;
// using English_games.Models;
// using English_games.Services;
// using FluentValidation;
// using Microsoft.AspNetCore.Builder;
// using Microsoft.AspNetCore.Hosting;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Identity;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.Logging;

using System.Text.Json.Serialization;
using English_games.Context;
using English_games.Models;
using English_games.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddHttpClient<MobizonApiService>(options =>
{
    options.Timeout = TimeSpan.FromSeconds(30);
});

string connection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<English_gamesContext>(options => options.UseSqlite(connection));

// Регистрация сервисов Identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // Параметры пароля
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;

    // Другие параметры Identity...

})
    .AddEntityFrameworkStores<English_gamesContext>()
    .AddDefaultTokenProviders();

// Регистрация сервиса для управления ошибками Identity
builder.Services.AddScoped<IdentityErrorDescriber, RuLocalizationDescriber>();

// Регистрация сервиса для работы с MobizonApiService
builder.Services.AddScoped<MobizonApiService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithRedirects("/Error/{0}");
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try
{
    var userManager = services.GetRequiredService<UserManager<User>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    // Применение миграций и обновление базы данных
    var dbContext = services.GetRequiredService<English_gamesContext>();
    dbContext.Database.Migrate();

    await AdminInitializer.SeedAdminUser(userManager, roleManager);
}
catch (Exception exception)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(exception, "Произошла ошибка при добавлении администратора");
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Main}/{action=Index}/{id?}");

app.Run();
