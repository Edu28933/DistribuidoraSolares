using System.Net;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using DistribuidoraSolares.Data;
using DistribuidoraSolares.Services;
using DistribuidoraSolares.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    // Agregar filtro global de autorización
    options.Filters.Add<GlobalAuthorizationFilter>();
});

// Configure sessions for authentication
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    // En Azure el balanceador termina HTTPS; la cookie debe ser Secure para que el navegador la envíe
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
});

// Configure Entity Framework
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    }));

// Register services
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IVentaService, VentaService>();
builder.Services.AddScoped<ICompraService, CompraService>();
builder.Services.AddScoped<IReservaService, ReservaService>();
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<IInventarioMovimientoService, InventarioMovimientoService>();
builder.Services.AddScoped<ICajaService, CajaService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IProveedorService, ProveedorService>();
builder.Services.AddScoped<IRolService, RolService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<ISucursalService, SucursalService>();
builder.Services.AddScoped<IMetodoPagoService, MetodoPagoService>();
builder.Services.AddScoped<IProductoFotoService, ProductoFotoService>();
builder.Services.AddScoped<ICodigoProductoService, CodigoProductoService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAzureBlobStorageService, AzureBlobStorageService>();
builder.Services.AddScoped<IUsuarioEmailService, UsuarioEmailService>();
builder.Services.AddScoped<IPermisoPantallaService, PermisoPantallaService>();

var app = builder.Build();

// En Azure el balanceador termina HTTPS y reenvía como HTTP; hay que confiar en X-Forwarded-Proto
var forwardedOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedOptions.KnownProxies.Clear();
forwardedOptions.KnownProxies.Add(IPAddress.Loopback);
forwardedOptions.KnownNetworks.Clear();
forwardedOptions.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(IPAddress.Loopback, 8)); // 127.0.0.0/8
forwardedOptions.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(IPAddress.Parse("10.0.0.0"), 8)); // Azure interno
app.UseForwardedHeaders(forwardedOptions);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable sessions (must be before UseAuthorization)
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();
