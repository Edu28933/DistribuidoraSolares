# RESUMEN: LO QUE FALTA PARA COMPLETAR MULTI-SUCURSAL

## ✅ COMPLETADO

1. ✅ Scripts SQL para stored procedures de consulta (mostrar/buscar) con `@SucursalId`
2. ✅ Servicios modificados para aceptar `SucursalId` opcional en consultas
3. ✅ Controladores modificados para filtrar por sucursal usando `SucursalHelper`
4. ✅ Servicios modificados para pasar `SucursalId` en operaciones de creación
5. ✅ Controladores modificados para obtener `SucursalId` de sesión y pasarlo a servicios
6. ✅ Scripts SQL completos para stored procedures de creación (`modificar-stored-procedures-creacion-completos.sql`)
7. ✅ Scripts SQL para stored procedures de anulación (`modificar-stored-procedures-anulacion-sucursal.sql`)
8. ✅ Scripts SQL para stored procedures de stock (`modificar-stored-procedures-stock-sucursal.sql`)
9. ✅ `StockService` modificado para aceptar `SucursalId`
10. ✅ Controladores modificados para pasar `SucursalId` a `StockService`
11. ✅ `ReservaService.LiberarProductosReservaParaVentaAsync` modificado para usar `SucursalId`

## ❌ PENDIENTE - STORED PROCEDURES DE CREACIÓN

### 1. `usp_venta_completa`
**Problema:** NO tiene `@SucursalId` y NO lo usa en:
- INSERT INTO `tbl_ventas` (falta `SucursalId`)
- INSERT INTO `tbl_inventario_movimientos` (falta `SucursalId`)

**Solución:**
```sql
-- Agregar parámetro
@SucursalId INT = NULL

-- Calcular SucursalId
DECLARE @SucursalIdCalculado INT;
IF @SucursalId IS NULL
BEGIN
    SELECT @SucursalIdCalculado = SucursalId 
    FROM tbl_usuarios 
    WHERE UsuarioId = @UsuarioId;
END
ELSE
BEGIN
    SET @SucursalIdCalculado = @SucursalId;
END

-- Usar en INSERT tbl_ventas
INSERT INTO dbo.tbl_ventas (..., SucursalId, ...)
VALUES (..., @SucursalIdCalculado, ...)

-- Usar en INSERT tbl_inventario_movimientos
INSERT INTO dbo.tbl_inventario_movimientos (..., SucursalId, ...)
VALUES (..., @SucursalIdCalculado, ...)
```

### 2. `usp_compra_completa`
**Problema:** NO tiene `@SucursalId` y NO lo usa en:
- INSERT INTO `tbl_compras` (falta `SucursalId`)
- INSERT INTO `tbl_inventario_movimientos` (falta `SucursalId`)

**Solución:** Mismo patrón que `usp_venta_completa`

### 3. `usp_reserva_crear_completa`
**Problema:** NO tiene `@SucursalId` y NO lo usa en:
- INSERT INTO `tbl_reservas` (falta `SucursalId`)
- INSERT INTO `tbl_inventario_movimientos` (falta `SucursalId`)

**Solución:** Mismo patrón que `usp_venta_completa`

### 4. `usp_caja_abrir`
**Problema:** NO tiene `@SucursalId` ni `@UsuarioId` y NO lo usa en:
- INSERT INTO `tbl_caja` (falta `SucursalId`)

**Solución:**
```sql
-- Agregar parámetros
@UsuarioId INT,
@SucursalId INT = NULL

-- Calcular SucursalId
DECLARE @SucursalIdCalculado INT;
IF @SucursalId IS NULL
BEGIN
    SELECT @SucursalIdCalculado = SucursalId 
    FROM tbl_usuarios 
    WHERE UsuarioId = @UsuarioId;
END
ELSE
BEGIN
    SET @SucursalIdCalculado = @SucursalId;
END

-- Usar en INSERT tbl_caja
INSERT INTO dbo.tbl_caja (..., SucursalId, ...)
VALUES (..., @SucursalIdCalculado, ...)
```

## ❌ PENDIENTE - STORED PROCEDURES DE ANULACIÓN/CANCELACIÓN

### 5. `usp_venta_anular`
**Problema:** INSERT INTO `tbl_inventario_movimientos` sin `SucursalId`

**Solución:** Obtener `SucursalId` de la venta anulada:
```sql
DECLARE @SucursalIdVenta INT;
SELECT @SucursalIdVenta = SucursalId 
FROM tbl_ventas 
WHERE VentaId = @VentaId;

-- Usar en INSERT tbl_inventario_movimientos
INSERT INTO dbo.tbl_inventario_movimientos (..., SucursalId, ...)
VALUES (..., @SucursalIdVenta, ...)
```

### 6. `usp_compra_anular`
**Problema:** INSERT INTO `tbl_inventario_movimientos` sin `SucursalId`

**Solución:** Obtener `SucursalId` de la compra anulada:
```sql
DECLARE @SucursalIdCompra INT;
SELECT @SucursalIdCompra = SucursalId 
FROM tbl_compras 
WHERE CompraId = @CompraId;

-- Usar en INSERT tbl_inventario_movimientos
INSERT INTO dbo.tbl_inventario_movimientos (..., SucursalId, ...)
VALUES (..., @SucursalIdCompra, ...)
```

### 7. `usp_reserva_cancelar`
**Problema:** INSERT INTO `tbl_inventario_movimientos` sin `SucursalId`

**Solución:** Obtener `SucursalId` de la reserva cancelada:
```sql
DECLARE @SucursalIdReserva INT;
SELECT @SucursalIdReserva = SucursalId 
FROM tbl_reservas 
WHERE ReservaId = @ReservaId;

-- Usar en INSERT tbl_inventario_movimientos
INSERT INTO dbo.tbl_inventario_movimientos (..., SucursalId, ...)
VALUES (..., @SucursalIdReserva, ...)
```

## ❌ PENDIENTE - STORED PROCEDURES DE INVENTARIO

### 8. `usp_inventario_movimientos_crear`
**Problema:** NO tiene `@SucursalId` y NO lo usa en INSERT

**Solución:**
```sql
-- Agregar parámetro
@SucursalId INT = NULL

-- Si es NULL, obtener del usuario
DECLARE @SucursalIdCalculado INT;
IF @SucursalId IS NULL
BEGIN
    SELECT @SucursalIdCalculado = SucursalId 
    FROM tbl_usuarios 
    WHERE UsuarioId = @UsuarioId;
END
ELSE
BEGIN
    SET @SucursalIdCalculado = @SucursalId;
END

-- Usar en INSERT
INSERT INTO dbo.tbl_inventario_movimientos (..., SucursalId, ...)
VALUES (..., @SucursalIdCalculado, ...)
```

### 9. `usp_compra_detalle_crear`
**Problema:** INSERT INTO `tbl_inventario_movimientos` sin `SucursalId`

**Solución:** Obtener `SucursalId` de la compra:
```sql
DECLARE @SucursalIdCompra INT;
SELECT @SucursalIdCompra = SucursalId 
FROM tbl_compras 
WHERE CompraId = @CompraId;

-- Usar en INSERT tbl_inventario_movimientos
INSERT INTO dbo.tbl_inventario_movimientos (..., SucursalId, ...)
VALUES (..., @SucursalIdCompra, ...)
```

## ❌ PENDIENTE - STORED PROCEDURES DE STOCK

### 10. `usp_stock_actual_general`
**Problema:** NO filtra por sucursal (calcula stock global)

**Solución:** Agregar `@SucursalId` opcional y filtrar movimientos:
```sql
@SucursalId INT = NULL

-- En el LEFT JOIN, agregar filtro
LEFT JOIN dbo.tbl_inventario_movimientos im
    ON im.ProductoId = p.ProductoId
    AND (@SucursalId IS NULL OR im.SucursalId = @SucursalId)
```

### 11. `usp_stock_actual_por_producto`
**Problema:** NO filtra por sucursal (calcula stock global)

**Solución:** Mismo patrón que `usp_stock_actual_general`

## ❌ PENDIENTE - SERVICIOS Y CONTROLADORES

### 12. `StockService`
**Problema:** NO acepta `SucursalId` para filtrar stock

**Solución:** Modificar métodos para aceptar `SucursalId` opcional y pasarlo a los stored procedures

### 13. Controladores que usan `StockService`
**Problema:** NO filtran stock por sucursal

**Solución:** Obtener `SucursalId` de sesión y pasarlo a `StockService`

## 📋 SCRIPTS SQL PARA EJECUTAR (EN ORDEN)

### 1. Scripts de Consulta (YA EJECUTADOS ✅)
- `modificar-stored-procedures-ventas-sucursal.sql`
- `modificar-stored-procedures-compras-sucursal.sql` ✅
- `modificar-stored-procedures-reservas-sucursal.sql`
- `modificar-stored-procedures-caja-sucursal.sql` ✅
- `modificar-stored-procedures-inventario-movimientos-sucursal.sql`

### 2. Scripts de Creación (NUEVOS - EJECUTAR AHORA)
- **`modificar-stored-procedures-creacion-completos.sql`** ⚠️ **CRÍTICO**
  - Modifica: `usp_venta_completa`, `usp_compra_completa`, `usp_reserva_crear_completa`, `usp_caja_abrir`
  - Agrega `@SucursalId` y lo usa en todos los INSERTs

### 3. Scripts de Anulación (NUEVOS - EJECUTAR AHORA)
- **`modificar-stored-procedures-anulacion-sucursal.sql`** ⚠️ **IMPORTANTE**
  - Modifica: `usp_venta_anular`, `usp_compra_anular`, `usp_reserva_cancelar`, `usp_inventario_movimientos_crear`, `usp_compra_detalle_crear`
  - Agrega `SucursalId` a movimientos de inventario en anulaciones

### 4. Scripts de Stock (NUEVOS - EJECUTAR AHORA)
- **`modificar-stored-procedures-stock-sucursal.sql`** ⚠️ **IMPORTANTE**
  - Modifica: `usp_stock_actual_general`, `usp_stock_actual_por_producto`
  - Agrega filtro por `@SucursalId`

## ✅ TODO COMPLETADO EN C#

Todos los servicios y controladores ya están modificados y listos para usar con los stored procedures actualizados.
