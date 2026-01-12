using Mapster;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Razor;
using CalendarioCorporativo.UI.Web;
using CalendarioCorporativo.UI.Web.Helpers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMvc()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();

builder.Services.AddHttpContextAccessor();
builder.Services.AddLocalization();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.SlidingExpiration = true;
        options.LoginPath = "/Admin/Login/EfetuarLoginAdmin";
        options.LogoutPath = "/Admin/Login/Sair";
        options.AccessDeniedPath = "/Admin/Home/Index";
        options.Cookie.Name = ".CALENDARIOCORPORATIVOADMIN";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(180);
    });

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

builder.Services.AddMapster();

DependencyContainer.RegisterContainers(builder.Services);
MappingConfig.RegisterMaps(builder.Services);

var app = builder.Build();

app.UseRequestLocalization(new RequestLocalizationOptions()
    .AddSupportedCultures(new[] { "pt-BR", "en-US" })
    .AddSupportedUICultures(new[] { "pt-BR", "en-US" }));

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Erro", true);
    app.UseStatusCodePages();
    app.UseHsts();
}

app.UsePathBase("/CalendarioCorporativo");
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapControllers();
app.UseCors();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapAreaControllerRoute(
    name: "Admin",
    areaName: "Admin",
    pattern: "Admin/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}");

app.Run();