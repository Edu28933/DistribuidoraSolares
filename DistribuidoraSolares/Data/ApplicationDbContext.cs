using Microsoft.EntityFrameworkCore;
using DistribuidoraSolares.Models;

namespace DistribuidoraSolares.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Producto> Productos { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Proveedor> Proveedores { get; set; }
    public DbSet<Rol> Roles { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Sucursal> Sucursales { get; set; }
    public DbSet<MetodoPago> MetodosPago { get; set; }
    public DbSet<Caja> Cajas { get; set; }
    public DbSet<CajaMovimiento> CajaMovimientos { get; set; }
    public DbSet<Venta> Ventas { get; set; }
    public DbSet<VentaDetalle> VentaDetalles { get; set; }
    public DbSet<Compra> Compras { get; set; }
    public DbSet<CompraDetalle> CompraDetalles { get; set; }
    public DbSet<Reserva> Reservas { get; set; }
    public DbSet<ReservaDetalle> ReservaDetalles { get; set; }
    public DbSet<InventarioMovimiento> InventarioMovimientos { get; set; }
    public DbSet<ProductoFoto> ProductoFotos { get; set; }
    public DbSet<UsuarioEmail> UsuarioEmails { get; set; }
    public DbSet<Pantalla> Pantallas { get; set; }
    public DbSet<PermisoPantalla> PermisosPantalla { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuraciones de tablas
        modelBuilder.Entity<Categoria>().ToTable("tbl_categorias");
        modelBuilder.Entity<Producto>().ToTable("tbl_productos");
        modelBuilder.Entity<Cliente>().ToTable("tbl_clientes");
        modelBuilder.Entity<Proveedor>().ToTable("tbl_proveedores");
        modelBuilder.Entity<Rol>().ToTable("tbl_roles");
        modelBuilder.Entity<Usuario>().ToTable("tbl_usuarios");
        modelBuilder.Entity<Sucursal>().ToTable("tbl_sucursales");
        modelBuilder.Entity<MetodoPago>().ToTable("tbl_metodos_pago");
        modelBuilder.Entity<Caja>().ToTable("tbl_caja");
        modelBuilder.Entity<CajaMovimiento>().ToTable("tbl_caja_movimientos");
        modelBuilder.Entity<Venta>().ToTable("tbl_ventas");
        modelBuilder.Entity<VentaDetalle>().ToTable("tbl_venta_detalle");
        modelBuilder.Entity<Compra>().ToTable("tbl_compras");
        modelBuilder.Entity<CompraDetalle>().ToTable("tbl_compra_detalle");
        modelBuilder.Entity<Reserva>().ToTable("tbl_reservas");
        modelBuilder.Entity<ReservaDetalle>().ToTable("tbl_reserva_detalle");
        modelBuilder.Entity<InventarioMovimiento>().ToTable("tbl_inventario_movimientos");
        modelBuilder.Entity<ProductoFoto>().ToTable("tbl_producto_fotos");
        modelBuilder.Entity<UsuarioEmail>().ToTable("tbl_usuario_emails");
        modelBuilder.Entity<Pantalla>().ToTable("tbl_pantallas");
        modelBuilder.Entity<PermisoPantalla>().ToTable("tbl_permisos_pantalla");

        // Configurar claves primarias
        modelBuilder.Entity<Categoria>().HasKey(c => c.CategoriaId);
        modelBuilder.Entity<Producto>().HasKey(p => p.ProductoId);
        modelBuilder.Entity<Cliente>().HasKey(c => c.ClienteId);
        modelBuilder.Entity<Proveedor>().HasKey(p => p.ProveedorId);
        modelBuilder.Entity<Rol>().HasKey(r => r.RolId);
        modelBuilder.Entity<Usuario>().HasKey(u => u.UsuarioId);
        modelBuilder.Entity<Sucursal>().HasKey(s => s.SucursalId);
        modelBuilder.Entity<MetodoPago>().HasKey(m => m.MetodoPagoId);
        modelBuilder.Entity<Caja>().HasKey(c => c.CajaId);
        modelBuilder.Entity<CajaMovimiento>().HasKey(c => c.MovimientoId);
        modelBuilder.Entity<Venta>().HasKey(v => v.VentaId);
        modelBuilder.Entity<VentaDetalle>().HasKey(v => v.VentaDetalleId);
        modelBuilder.Entity<Compra>().HasKey(c => c.CompraId);
        modelBuilder.Entity<CompraDetalle>().HasKey(c => c.CompraDetalleId);
        modelBuilder.Entity<Reserva>().HasKey(r => r.ReservaId);
        modelBuilder.Entity<ReservaDetalle>().HasKey(r => r.ReservaDetalleId);
        modelBuilder.Entity<InventarioMovimiento>().HasKey(i => i.InvMovId);
        modelBuilder.Entity<ProductoFoto>().HasKey(p => p.FotoId);
        modelBuilder.Entity<UsuarioEmail>().HasKey(u => u.UsuarioEmailId);
        modelBuilder.Entity<Pantalla>().HasKey(p => p.PantallaId);
        modelBuilder.Entity<PermisoPantalla>().HasKey(p => p.PermisoPantallaId);
    }
}