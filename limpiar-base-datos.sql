-- =============================================
-- LIMPIAR BASE DE DATOS (bd_distribuidora_solares)
-- =============================================
-- IMPORTANTE: Hacer BACKUP de la base antes de ejecutar.
--
-- OPCIÓN B (recomendada): Solo datos operativos.
--   Se borran: ventas, compras, caja, movimientos, reservas, permisos.
--   Se mantienen: categorías, productos, clientes, proveedores,
--   usuarios, roles, sucursales, métodos de pago, pantallas.
--
-- OPCIÓN A: Vaciar TODO (todas las tablas).
-- =============================================

USE [bd_distribuidora_solares];
GO

-- Opción por defecto: B (solo datos operativos). Cambia a 'A' si quieres vaciar todo.
DECLARE @Opcion CHAR(1) = 'B';

-- =============================================
-- OPCIÓN A: VACIAR TODO (todas las tablas)
-- =============================================
IF @Opcion = 'A'
BEGIN
    -- Deshabilitar FKs para poder truncar en cualquier orden
    EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';

    -- Borrar en orden (hijos antes que padres)
    DELETE FROM tbl_venta_detalle;
    DELETE FROM tbl_compra_detalle;
    DELETE FROM tbl_reserva_detalle;
    DELETE FROM tbl_inventario_movimientos;
    DELETE FROM tbl_caja_movimientos;
    DELETE FROM tbl_permisos_pantalla;
    DELETE FROM tbl_usuario_emails;
    DELETE FROM tbl_producto_fotos;
    DELETE FROM tbl_ventas;
    DELETE FROM tbl_compras;
    DELETE FROM tbl_reservas;
    DELETE FROM tbl_caja;
    DELETE FROM tbl_usuarios;
    DELETE FROM tbl_productos;
    DELETE FROM tbl_clientes;
    DELETE FROM tbl_proveedores;
    DELETE FROM tbl_metodos_pago;
    DELETE FROM tbl_sucursales;
    DELETE FROM tbl_roles;
    DELETE FROM tbl_categorias;
    DELETE FROM tbl_pantallas;

    -- Volver a habilitar FKs
    EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL';

    PRINT 'Opción A: Todas las tablas han sido vaciadas.';
END

-- =============================================
-- OPCIÓN B: SOLO DATOS OPERATIVOS
-- (se mantienen: categorías, productos, clientes, proveedores, usuarios, roles, sucursales, métodos de pago, pantallas)
-- =============================================
IF @Opcion = 'B'
BEGIN
    DELETE FROM tbl_venta_detalle;
    DELETE FROM tbl_compra_detalle;
    DELETE FROM tbl_reserva_detalle;
    DELETE FROM tbl_inventario_movimientos;
    DELETE FROM tbl_caja_movimientos;
    DELETE FROM tbl_permisos_pantalla;
    DELETE FROM tbl_ventas;
    DELETE FROM tbl_compras;
    DELETE FROM tbl_reservas;
    DELETE FROM tbl_caja;

    PRINT 'Opción B: Datos operativos eliminados. Catálogos y usuarios se mantienen.';
END

GO

PRINT 'Ejecución finalizada. Revisa los resultados antes de cerrar.';
GO
