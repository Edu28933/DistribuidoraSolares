# PROGRESO IMPLEMENTACIÓN MULTI-SUCURSAL

## ✅ COMPLETADO

### 1. Base de Datos
- ✅ Script de migración SQL (`migracion-multi-sucursal.sql`)
  - Tabla `tbl_sucursales` creada
  - Sucursal "Tienda 1" creada automáticamente
  - `SucursalId` agregado a: `tbl_usuarios`, `tbl_ventas`, `tbl_compras`, `tbl_reservas`, `tbl_caja`, `tbl_inventario_movimientos`
  - **NOTA**: `tbl_productos` NO tiene `SucursalId` (productos son globales)
  - Datos existentes asignados a "Tienda 1"

### 2. Stored Procedures
- ✅ `stored-procedures-sucursales.sql`
  - `usp_sucursales_mostrar`
  - `usp_sucursales_crear`
  - `usp_sucursales_editar`
  - `usp_sucursales_eliminar`
  - `usp_sucursales_buscar`
  - `usp_sucursales_obtener_por_id`
- ✅ `stored-procedures-stock-sucursal.sql`
  - `usp_stock_por_producto_sucursal` - Stock de un producto en todas las sucursales
  - `usp_stock_general_por_sucursal` - Stock general por sucursal

### 3. Modelos C#
- ✅ `Sucursal.cs` creado
- ✅ `Usuario.cs` - Agregado `SucursalId` y propiedad de navegación `Sucursal`
- ✅ `Venta.cs` - Agregado `SucursalId` y propiedad de navegación `Sucursal`
- ✅ `Compra.cs` - Agregado `SucursalId` y propiedad de navegación `Sucursal`
- ✅ `Reserva.cs` - Agregado `SucursalId` y propiedad de navegación `Sucursal`
- ✅ `Caja.cs` - Agregado `SucursalId`
- ✅ `InventarioMovimiento.cs` - Agregado `SucursalId` y propiedad de navegación `Sucursal`
- ✅ `Producto.cs` - **NO modificado** (productos son globales)
- ✅ `ApplicationDbContext.cs` - Agregado `DbSet<Sucursal>`

### 4. Servicios
- ✅ `SucursalService.cs` creado con interfaz `ISucursalService`
- ✅ `SucursalHelper.cs` - Helper para obtener `SucursalId` según rol
- ✅ Registrado en `Program.cs`

### 5. Autenticación
- ✅ `LoginController.cs` modificado para guardar `SucursalId` en sesión
- ✅ Sesión guarda: `SucursalId` y `SucursalNombre`

---

## ⏳ PENDIENTE

### 1. Controladores
- [ ] Crear `SucursalesController` con CRUD completo
- [ ] Modificar `UsuariosController` para incluir selector de sucursal
- [ ] Modificar controladores existentes para filtrar por sucursal:
  - [ ] `VentasController`
  - [ ] `ComprasController`
  - [ ] `ReservasController`
  - [ ] `ProductosController` (para mostrar stock por sucursal)
  - [ ] `CajaController`
  - [ ] `MovimientosInventarioController`

### 2. Servicios - Filtrar por Sucursal
- [ ] Modificar `VentaService` para filtrar por `SucursalId`
- [ ] Modificar `CompraService` para filtrar por `SucursalId`
- [ ] Modificar `ReservaService` para filtrar por `SucursalId`
- [ ] Modificar `StockService` para calcular stock por `SucursalId`
- [ ] Modificar `InventarioMovimientoService` para filtrar por `SucursalId`
- [ ] Modificar `CajaService` para filtrar por `SucursalId`
- [ ] Modificar `ProductoService` para mostrar stock por sucursal (opcional)

### 3. Stored Procedures - Agregar parámetro @SucursalId
- [ ] `usp_ventas_mostrar` - Agregar `@SucursalId` opcional
- [ ] `usp_compras_mostrar` - Agregar `@SucursalId` opcional
- [ ] `usp_reservas_mostrar` - Agregar `@SucursalId` opcional
- [ ] `usp_caja_mostrar` - Agregar `@SucursalId` opcional
- [ ] `usp_inventario_movimientos_mostrar` - Agregar `@SucursalId` opcional
- [ ] `usp_stock_actual_general` - Agregar `@SucursalId` opcional
- [ ] `usp_stock_actual_por_producto` - Agregar `@SucursalId` opcional
- [ ] `usp_productos_mostrar` - Agregar `@SucursalId` opcional (para filtrar productos con stock en esa sucursal)

### 4. Stored Procedures - Modificar creación para incluir SucursalId
- [ ] `usp_venta_completa` - Agregar `@SucursalId`
- [ ] `usp_compra_completa` - Agregar `@SucursalId`
- [ ] `usp_reserva_crear_completa` - Agregar `@SucursalId`
- [ ] `usp_caja_abrir` - Agregar `@SucursalId`
- [ ] `usp_inventario_movimientos_crear` (si existe) - Agregar `@SucursalId`

### 5. Vistas
- [ ] Crear vistas para `SucursalesController`:
  - [ ] `Index.cshtml` - Lista de sucursales
  - [ ] `Create.cshtml` - Crear sucursal
  - [ ] `Edit.cshtml` - Editar sucursal
- [ ] Modificar `Usuarios/Create.cshtml` y `Edit.cshtml` - Agregar selector de sucursal
- [ ] Modificar vistas de creación para incluir selector de sucursal:
  - [ ] `Ventas/Create.cshtml` - Selector de sucursal (auto según usuario)
  - [ ] `Compras/Create.cshtml` - Selector de sucursal
  - [ ] `Reservas/Create.cshtml` - Selector de sucursal
  - [ ] `Caja/Abrir.cshtml` - Selector de sucursal
- [ ] Modificar vistas de listado para agregar filtro de sucursal:
  - [ ] `Ventas/Index.cshtml` - Dropdown filtro sucursal
  - [ ] `Compras/Index.cshtml` - Dropdown filtro sucursal
  - [ ] `Reservas/Index.cshtml` - Dropdown filtro sucursal
  - [ ] `Productos/Index.cshtml` - Mostrar stock por sucursal
  - [ ] `Caja/Index.cshtml` - Dropdown filtro sucursal
- [ ] Agregar vista para ver stock de producto por sucursal (nueva funcionalidad)

### 6. Lógica de Negocio
- [ ] Modificar creación de ventas para asignar `SucursalId` del usuario
- [ ] Modificar creación de compras para asignar `SucursalId` del usuario
- [ ] Modificar creación de reservas para asignar `SucursalId` del usuario
- [ ] Modificar apertura de caja para asignar `SucursalId` del usuario
- [ ] Validar que solo haya una caja abierta por sucursal
- [ ] Modificar movimientos de inventario para incluir `SucursalId`

### 7. Validaciones
- [ ] Validar que Admin/Vendedor solo puedan crear ventas/compras/reservas en su sucursal
- [ ] Validar que SuperAdmin/Contador puedan seleccionar cualquier sucursal
- [ ] Validar stock por sucursal al crear ventas/reservas

---

## 📋 ORDEN DE IMPLEMENTACIÓN RECOMENDADO

1. **Ejecutar scripts SQL** (migración y stored procedures)
2. **Crear SucursalesController y vistas básicas**
3. **Modificar UsuariosController** para asignar sucursales
4. **Modificar servicios** para filtrar por sucursal
5. **Modificar stored procedures** para aceptar `@SucursalId`
6. **Modificar controladores** para usar filtrado por sucursal
7. **Modificar vistas** para mostrar/editar sucursal
8. **Implementar vista de stock por sucursal**

---

## ⚠️ NOTAS IMPORTANTES

1. **Productos son globales**: No tienen `SucursalId`. El stock se calcula desde `tbl_inventario_movimientos` que SÍ tiene `SucursalId`.

2. **Stock por sucursal**: Se calcula sumando movimientos de inventario con `Estado = 'ACTIVO'` filtrados por `ProductoId` y `SucursalId`.

3. **SuperAdmin y Contador**: Tienen `SucursalId = NULL` y ven todo. El helper `SucursalHelper.ObtenerSucursalIdParaFiltro` retorna `null` para estos roles.

4. **Admin y Vendedor**: Tienen `SucursalId` asignado y solo ven datos de su sucursal.

5. **Vista de stock por sucursal**: El usuario pidió poder ver el stock de cada producto en las diferentes sucursales. Esto se puede hacer con el stored procedure `usp_stock_por_producto_sucursal`.

---

## 🚀 PRÓXIMOS PASOS INMEDIATOS

1. Ejecutar los scripts SQL de migración
2. Crear `SucursalesController`
3. Crear vistas básicas de sucursales
4. Modificar `UsuariosController` para asignar sucursales
