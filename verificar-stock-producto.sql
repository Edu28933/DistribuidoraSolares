-- Script para verificar el cálculo de stock de un producto
-- Reemplaza @ProductoId con el ID del producto que quieres verificar (ej: 1 para Guardapolvo)

DECLARE @ProductoId INT = 1; -- Cambia este valor según el producto que quieras verificar

-- Ver movimientos de inventario del producto
SELECT 
    InvMovId,
    FechaHora,
    Tipo,
    Cantidad,
    Estado,
    VentaId,
    CompraId,
    ReservaId,
    Descripcion
FROM tbl_inventario_movimientos
WHERE ProductoId = @ProductoId
ORDER BY FechaHora DESC;

-- Calcular stock manualmente
SELECT 
    @ProductoId AS ProductoId,
    ISNULL(SUM(CASE WHEN Tipo = 'COMPRA' THEN Cantidad ELSE 0 END), 0) AS TotalCompras,
    ISNULL(SUM(CASE WHEN Tipo IN ('VENTA', 'PERDIDA') THEN Cantidad ELSE 0 END), 0) AS TotalVentasPerdidas,
    ISNULL(SUM(CASE WHEN Tipo = 'COMPRA' THEN Cantidad ELSE 0 END), 0) - 
    ISNULL(SUM(CASE WHEN Tipo IN ('VENTA', 'PERDIDA') THEN Cantidad ELSE 0 END), 0) AS StockCalculado
FROM tbl_inventario_movimientos
WHERE ProductoId = @ProductoId
  AND Estado = 'ACTIVO';

-- Ver reservas activas del producto
SELECT 
    r.ReservaId,
    r.ClienteId,
    c.Nombre AS ClienteNombre,
    rd.CantidadReservada,
    rd.Estado AS EstadoDetalle,
    r.Estado AS EstadoReserva
FROM tbl_reserva_detalle rd
INNER JOIN tbl_reservas r ON rd.ReservaId = r.ReservaId
INNER JOIN tbl_clientes c ON r.ClienteId = c.ClienteId
WHERE rd.ProductoId = @ProductoId
  AND rd.Estado IN ('ACTIVO', 'OCUPADO')
  AND r.Estado = 'ACTIVA';

-- Comparar con el stored procedure
EXEC usp_stock_actual_por_producto @ProductoId;
