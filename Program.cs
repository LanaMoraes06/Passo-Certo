using Npgsql;
using PassoCerto.Web.Repositories;
using PassoCerto.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using PassoCerto.Web.Services;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToPage("/Auth/Login");
    options.Conventions.AllowAnonymousToPage("/Auth/AcessoNegado");
    options.Conventions.AllowAnonymousToPage("/Error");

    options.Conventions.AuthorizeFolder(
        "/Admin",
        "SomenteAdministrador");
});

builder.Services.AddNpgsqlDataSource(
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Conexão com PostgreSQL não configurada.")
);


builder.Services.AddScoped<
    IUsuarioRepository,
    UsuarioRepository>();

builder.Services.AddScoped<
    IRegiaoRepository,
    RegiaoRepository>();

builder.Services.AddScoped<
    ILojaRepository,
    LojaRepository>();


builder.Services.AddScoped<
    IChecklistRepository,
    ChecklistRepository>();


builder.Services.AddScoped<
    IInspecaoRepository,
    InspecaoRepository>();

builder.Services.AddScoped<
    IDashboardRepository,
    DashboardRepository>();

builder.Services.AddScoped<AuthService>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AcessoNegado";        

        options.Cookie.Name = "PassoCerto.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("PodeInspecionar", policy =>
    {
        policy.RequireRole("SUPERVISOR");
    })
    .AddPolicy("PodeConsultarRelatorios", policy =>
    {
        policy.RequireRole(
            "SUPERVISOR",
            "DIRETORIA",
            "ADMINISTRADOR"
        );
    })
    .AddPolicy("SomenteDiretoria", policy =>
    {
        policy.RequireRole("DIRETORIA");
    
    }).AddPolicy("SomenteAdministrador", policy =>
    {

    policy.RequireAuthenticatedUser();
    policy.RequireRole("ADMINISTRADOR");
    });
    

    builder.Services.AddScoped<DashboardService>();
    builder.Services.AddScoped<ChecklistService>();
    builder.Services.AddScoped<InspecaoService>();
    builder.Services.AddScoped<LojaService>();

    
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();