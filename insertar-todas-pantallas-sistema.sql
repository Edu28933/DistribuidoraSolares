-- =============================================
-- INSERTAR PANTALLAS POR MÓDULO (UNA POR CONTROLADOR)
-- Una fila por módulo; las columnas Puede Ver / Crear / Editar / Eliminar
-- controlan las acciones de ese módulo.
-- Ejecutar en bd_distribuidora_solares.
-- =============================================

USE [bd_distribuidora_solares];
GO

INSERT INTO tbl_pantallas (Nombre, Controlador, Accion, Descripcion, Estado)
SELECT s.Nombre, s.Controlador, s.Accion, s.Descripcion, 'ACTIVO'
FROM (VALUES
    (N'Dashboard', N'Dashboard', N'Index', N'Panel principal'),
    (N'Categorías', N'Categorias', N'Index', N'Gestión de categorías'),
    (N'Productos', N'Productos', N'Index', N'Gestión de productos'),
    (N'Compras', N'Compras', N'Index', N'Gestión de compras'),
    (N'Ventas', N'Ventas', N'Index', N'Gestión de ventas'),
    (N'Reservas', N'Reservas', N'Index', N'Gestión de reservas'),
    (N'Caja', N'Caja', N'Index', N'Gestión de caja'),
    (N'Clientes', N'Clientes', N'Index', N'Gestión de clientes'),
    (N'Proveedores', N'Proveedores', N'Index', N'Gestión de proveedores'),
    (N'Usuarios', N'Usuarios', N'Index', N'Gestión de usuarios'),
    (N'Roles', N'Roles', N'Index', N'Gestión de roles'),
    (N'Métodos de Pago', N'MetodosPago', N'Index', N'Gestión de métodos de pago'),
    (N'Movimientos de Inventario', N'MovimientosInventario', N'Index', N'Movimientos de inventario'),
    (N'Permisos por Pantalla', N'Permisos', N'Index', N'Gestión de permisos por rol'),
    (N'Reportes', N'Reportes', N'Index', N'Reportes del sistema'),
    (N'Sucursales', N'Sucursales', N'Index', N'Gestión de sucursales'),
    (N'Stock por Sucursal', N'StockPorSucursal', N'Index', N'Stock por sucursal'),
    (N'Transferir Productos', N'TransferenciasProductos', N'Index', N'Transferencias entre sucursales')
) AS s(Nombre, Controlador, Accion, Descripcion)
WHERE NOT EXISTS (
    SELECT 1 FROM tbl_pantallas p
    WHERE p.Controlador = s.Controlador AND ISNULL(p.Accion, '') = ISNULL(s.Accion, '')
);
GO

PRINT 'Pantallas por módulo listas. Una fila por controlador; Ver/Crear/Editar/Eliminar aplican a ese módulo.';
GO
