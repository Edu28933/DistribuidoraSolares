-- =============================================
-- Agregar pantallas faltantes para Gestión de Permisos
-- Ejecutar en bd_distribuidora_solares para que aparezcan
-- Reportes, Sucursales, Stock por Sucursal y Transferir Productos.
-- =============================================

USE [bd_distribuidora_solares];
GO

-- Insertar solo si no existe (Controlador, Accion)
INSERT INTO tbl_pantallas (Nombre, Controlador, Accion, Descripcion, Estado)
SELECT s.Nombre, s.Controlador, s.Accion, s.Descripcion, 'ACTIVO'
FROM (VALUES
    (N'Reportes', N'Reportes', N'Index', N'Reportes del sistema'),
    (N'Sucursales', N'Sucursales', N'Index', N'Gestión de sucursales'),
    (N'Stock por Sucursal', N'StockPorSucursal', N'Index', N'Stock de productos por sucursal'),
    (N'Transferir Productos', N'TransferenciasProductos', N'Index', N'Transferencias entre sucursales')
) AS s(Nombre, Controlador, Accion, Descripcion)
WHERE NOT EXISTS (
    SELECT 1 FROM tbl_pantallas p
    WHERE p.Controlador = s.Controlador AND ISNULL(p.Accion, '') = ISNULL(s.Accion, '')
);
GO

PRINT 'Pantallas faltantes agregadas. Verificar con: SELECT * FROM tbl_pantallas ORDER BY Nombre;';
GO
