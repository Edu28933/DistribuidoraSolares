-- Script para recuperar productos de reserva que fueron vendidos antes de la corrección
-- Este script identifica reservas convertidas que no tienen movimientos LIBERACION_RESERVA
-- y los crea para corregir el stock físico

USE [bd_distribuidora_solares];
GO

-- Paso 1: Identificar reservas convertidas con ventas pero sin movimientos LIBERACION_RESERVA
SELECT 
    r.ReservaId,
    r.ClienteId,
    r.Estado AS EstadoReserva,
    COUNT(DISTINCT v.VentaId) AS VentasAsociadas,
    COUNT(DISTINCT im_lib.InvMovId) AS MovimientosLiberacionExistentes
FROM tbl_reservas r
INNER JOIN tbl_reserva_detalle rd ON r.ReservaId = rd.ReservaId
LEFT JOIN tbl_ventas v ON v.ClienteId = r.ClienteId 
    AND v.FechaHora >= r.FechaHora 
    AND v.Estado = 'ACTIVO'
LEFT JOIN tbl_inventario_movimientos im_lib ON im_lib.ReservaId = r.ReservaId 
    AND im_lib.Tipo = 'LIBERACION_RESERVA'
    AND im_lib.Estado = 'ACTIVO'
WHERE r.Estado = 'CONVERTIDA'
GROUP BY r.ReservaId, r.ClienteId, r.Estado
HAVING COUNT(DISTINCT im_lib.InvMovId) = 0
ORDER BY r.ReservaId;
GO

-- Paso 2: Ver detalles de reserva que están marcados como OCUPADO pero no tienen LIBERACION_RESERVA
SELECT 
    rd.ReservaDetalleId,
    rd.ReservaId,
    rd.ProductoId,
    p.Nombre AS ProductoNombre,
    rd.CantidadReservada,
    rd.Estado AS EstadoDetalle,
    r.Estado AS EstadoReserva,
    COUNT(im_lib.InvMovId) AS MovimientosLiberacion
FROM tbl_reserva_detalle rd
INNER JOIN tbl_reservas r ON rd.ReservaId = r.ReservaId
INNER JOIN tbl_productos p ON rd.ProductoId = p.ProductoId
LEFT JOIN tbl_inventario_movimientos im_lib ON im_lib.ReservaId = rd.ReservaId
    AND im_lib.ProductoId = rd.ProductoId
    AND im_lib.Tipo = 'LIBERACION_RESERVA'
    AND im_lib.Estado = 'ACTIVO'
WHERE rd.Estado = 'OCUPADO'
    AND r.Estado = 'CONVERTIDA'
GROUP BY rd.ReservaDetalleId, rd.ReservaId, rd.ProductoId, p.Nombre, rd.CantidadReservada, rd.Estado, r.Estado
HAVING COUNT(im_lib.InvMovId) = 0
ORDER BY rd.ReservaId, rd.ProductoId;
GO

-- Paso 3: CORRECCIÓN - Crear movimientos LIBERACION_RESERVA faltantes
-- IMPORTANTE: Revisa los resultados de los pasos anteriores antes de ejecutar esto
-- Este script crea los movimientos LIBERACION_RESERVA para productos OCUPADOS sin liberación

DECLARE @ReservaId INT;
DECLARE @ProductoId INT;
DECLARE @CantidadReservada INT;
DECLARE @UsuarioId INT = 1; -- CAMBIA ESTE VALOR por el UsuarioId correcto
DECLARE @VentaId INT;

-- Cursor para procesar cada detalle de reserva ocupado sin liberación
DECLARE reservas_cursor CURSOR FOR
SELECT DISTINCT
    rd.ReservaId,
    rd.ProductoId,
    rd.CantidadReservada,
    (SELECT TOP 1 v.VentaId 
     FROM tbl_ventas v 
     WHERE v.ClienteId = r.ClienteId 
       AND v.FechaHora >= r.FechaHora 
       AND v.Estado = 'ACTIVO'
     ORDER BY v.FechaHora DESC) AS VentaId
FROM tbl_reserva_detalle rd
INNER JOIN tbl_reservas r ON rd.ReservaId = r.ReservaId
WHERE rd.Estado = 'OCUPADO'
  AND r.Estado = 'CONVERTIDA'
  AND NOT EXISTS (
      SELECT 1 
      FROM tbl_inventario_movimientos im
      WHERE im.ReservaId = rd.ReservaId
        AND im.ProductoId = rd.ProductoId
        AND im.Tipo = 'LIBERACION_RESERVA'
        AND im.Estado = 'ACTIVO'
  );

OPEN reservas_cursor;
FETCH NEXT FROM reservas_cursor INTO @ReservaId, @ProductoId, @CantidadReservada, @VentaId;

WHILE @@FETCH_STATUS = 0
BEGIN
    PRINT 'Procesando ReservaId: ' + CAST(@ReservaId AS VARCHAR(10)) + 
          ', ProductoId: ' + CAST(@ProductoId AS VARCHAR(10)) + 
          ', Cantidad: ' + CAST(@CantidadReservada AS VARCHAR(10)) +
          ', VentaId: ' + ISNULL(CAST(@VentaId AS VARCHAR(10)), 'NULL');
    
    -- Crear movimiento LIBERACION_RESERVA
    INSERT INTO tbl_inventario_movimientos
    (ProductoId, FechaHora, Tipo, Cantidad, CostoUnitario, VentaId, CompraId, ReservaId, Descripcion, UsuarioId, Estado)
    VALUES
    (@ProductoId, 
     GETDATE(), 
     'LIBERACION_RESERVA', 
     @CantidadReservada, -- Cantidad positiva para liberar
     NULL, 
     @VentaId, 
     NULL, 
     @ReservaId, 
     'Liberación de reserva por venta #' + ISNULL(CAST(@VentaId AS VARCHAR(10)), 'N/A') + ' (Recuperación manual)', 
     @UsuarioId, 
     'ACTIVO');
    
    PRINT 'Movimiento LIBERACION_RESERVA creado exitosamente';
    
    FETCH NEXT FROM reservas_cursor INTO @ReservaId, @ProductoId, @CantidadReservada, @VentaId;
END

CLOSE reservas_cursor;
DEALLOCATE reservas_cursor;

PRINT 'Proceso de recuperación completado';
GO

-- Paso 4: Verificar el resultado - Stock después de la corrección
SELECT 
    p.ProductoId,
    p.Nombre AS ProductoNombre,
    ISNULL(SUM(CASE WHEN im.Estado = 'ACTIVO' THEN im.Cantidad ELSE 0 END), 0) AS StockFisicoActual,
    COUNT(DISTINCT CASE WHEN rd.Estado = 'ACTIVO' THEN rd.ReservaId END) AS ReservasActivas,
    SUM(CASE WHEN rd.Estado = 'ACTIVO' THEN rd.CantidadReservada ELSE 0 END) AS CantidadReservadaActiva,
    COUNT(DISTINCT CASE WHEN rd.Estado = 'OCUPADO' THEN rd.ReservaId END) AS ReservasOcupadas,
    SUM(CASE WHEN rd.Estado = 'OCUPADO' THEN rd.CantidadReservada ELSE 0 END) AS CantidadReservadaOcupada
FROM tbl_productos p
LEFT JOIN tbl_inventario_movimientos im ON im.ProductoId = p.ProductoId
LEFT JOIN tbl_reserva_detalle rd ON rd.ProductoId = p.ProductoId
LEFT JOIN tbl_reservas r ON rd.ReservaId = r.ReservaId AND r.Estado = 'CONVERTIDA'
WHERE EXISTS (
    SELECT 1 
    FROM tbl_reserva_detalle rd2 
    WHERE rd2.ProductoId = p.ProductoId 
      AND rd2.Estado = 'OCUPADO'
)
GROUP BY p.ProductoId, p.Nombre
ORDER BY p.Nombre;
GO
