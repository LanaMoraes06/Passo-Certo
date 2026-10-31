using Npgsql;
using PassoCerto.Web.Repositories;
using PassoCerto.Web.Repositories.Interfaces;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();


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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
