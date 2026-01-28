# INSTRUCCIONES PARA CORREGIR STOCK Y PRECIOS DE VENTA

## PROBLEMA 1: Error "StockMinimo" en Transferencias de Productos

**Error:** `The required column 'StockMinimo' was not present in the results of a 'FromSql' operation.`

**Solución:** Ejecutar el script `corregir-stock-actual-por-producto.sql`

Este script corrige el stored procedure `usp_stock_actual_por_producto` para que devuelva la columna `StockMinimo` que el modelo `StockActual` requiere.

## PROBLEMA 2: No se muestra el costo total de ventas en Movimientos de Inventario

**Solución:** Ejecutar el script `modificar-stored-procedures-inventario-movimientos-precio-venta.sql`

Este script modifica los stored procedures de movimientos de inventario para:
- Obtener el precio de venta desde `tbl_venta_detalle` cuando el movimiento es de tipo `VENTA`
- Mostrar el costo unitario y costo total en cada línea de venta
- Actualizar el cálculo del total de ventas en el resumen

## PASOS A SEGUIR:

1. **Ejecutar `corregir-stock-actual-por-producto.sql`** en tu base de datos
2. **Ejecutar `modificar-stored-procedures-inventario-movimientos-precio-venta.sql`** en tu base de datos
3. **Reiniciar la aplicación** para que los cambios surtan efecto
4. **Probar:**
   - Ir a "Stock por Sucursal" y verificar que se muestre correctamente
   - Ir a "Transferir Productos" y verificar que funcione sin errores
   - Ir a "Movimientos de Inventario" y verificar que se muestren los valores de ventas

## NOTA IMPORTANTE:

El código C# ya está implementado correctamente. Los problemas son solo en los stored procedures de la base de datos que necesitan ser actualizados.
