using APP.Data;
using APP.Services; // Asegúrate de tener esto si usas IExcelExportService
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// ================== Servicios ==================

// MVC con vistas
builder.Services.AddControllersWithViews();

// Servicio de exportación a Excel
builder.Services.AddScoped<IExcelExportService, ExcelExportService>();

// Servicio de acceso a base de datos personalizado
builder.Services.AddScoped<ConexionMySql>();

// Acceso al contexto HTTP (útil para filtros, layouts, etc.)
builder.Services.AddHttpContextAccessor();

// Configuración de sesiones en memoria
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// ================== Middleware ==================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Las sesiones deben ir antes de autorización
app.UseSession();

app.UseAuthorization();

// ================== Rutas ==================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
