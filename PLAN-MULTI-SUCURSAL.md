# PLAN DETALLADO: IMPLEMENTACIÓN MULTI-SUCURSAL
## Sistema de Gestión Distribuidora Solares

---

## 📋 RESUMEN EJECUTIVO

Este documento describe los pasos necesarios para convertir el sistema actual (mono-sucursal) en un sistema multi-sucursal con las siguientes características:

- **SuperAdmin**: Ve todo de todas las sucursales
- **Admin por Sucursal**: Ve solo su sucursal asignada
- **Vendedor**: Ve solo su sucursal asignada
- **Contador**: Ve todo para contabilidad (reportes consolidados)

---

## 🎯 OBJETIVOS

1. Crear estructura de sucursales en base de datos
2. Asignar usuarios a sucursales
3. Filtrar datos por sucursal según rol
4. Mantener stock independiente por sucursal
5. Permitir transferencias entre sucursales (opcional futuro)
6. Reportes consolidados para SuperAdmin y Contador

---

## 📊 FASES DE IMPLEMENTACIÓN

### **FASE 1: PREPARACIÓN Y ANÁLISIS** ⚠️ CRÍTICO

#### 1.1 Backup de Base de Datos
- [ ] **HACER BACKUP COMPLETO** de la base de datos antes de cualquier cambio
- [ ] Documentar versión actual del esquema
- [ ] Crear script de rollback por si algo sale mal

#### 1.2 Análisis de Datos Existentes
- [ ] Identificar todos los registros actuales (ventas, compras, reservas, etc.)
- [ ] Decidir a qué sucursal asignar los datos históricos (probablemente "Sucursal Principal" o "Sucursal 1")
- [ ] Documentar cantidad de registros por tabla

#### 1.3 Definición de Roles
- [ ] Confirmar nombres exactos de roles:
  - SuperAdmin (ya existe)
  - Admin (ya existe, pero necesitará asignación de sucursal)
  - Vendedor (ya existe, pero necesitará asignación de sucursal)
  - Contador (¿existe o hay que crearlo?)
- [ ] Definir permisos específicos por rol

---

### **FASE 2: CAMBIOS EN BASE DE DATOS**

#### 2.1 Crear Tabla de Sucursales
```sql
CREATE TABLE tbl_sucursales (
    SucursalId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(150) NOT NULL,
    Codigo VARCHAR(20) NULL, -- Código único (ej: "SUC001")
    Direccion VARCHAR(255) NULL,
    Telefono VARCHAR(30) NULL,
    Estado VARCHAR(15) NOT NULL DEFAULT 'ACTIVO', -- ACTIVO / INACTIVO
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE()
);
```

#### 2.2 Modificar Tabla de Usuarios
```sql
ALTER TABLE tbl_usuarios
ADD SucursalId INT NULL; -- NULL = SuperAdmin/Contador (ve todo)

ALTER TABLE tbl_usuarios
ADD CONSTRAINT FK_usuarios_sucursales 
FOREIGN KEY (SucursalId) REFERENCES tbl_sucursales(SucursalId);
```

#### 2.3 Agregar SucursalId a Tablas Principales

**Tablas que NECESITAN SucursalId:**
- `tbl_productos` - Stock por sucursal (CRÍTICO)
- `tbl_ventas` - Ventas por sucursal
- `tbl_compras` - Compras por sucursal
- `tbl_reservas` - Reservas por sucursal
- `tbl_caja` - Caja por sucursal
- `tbl_inventario_movimientos` - Movimientos por sucursal

**Tablas que NO necesitan SucursalId (compartidas):**
- `tbl_clientes` - Clientes son globales
- `tbl_proveedores` - Proveedores son globales
- `tbl_categorias` - Categorías son globales
- `tbl_metodos_pago` - Métodos de pago son globales
- `tbl_roles` - Roles son globales

#### 2.4 Scripts de Migración de Datos
- [ ] Crear script para asignar todos los datos existentes a "Sucursal Principal"
- [ ] Crear script para asignar usuarios existentes a "Sucursal Principal" (excepto SuperAdmin)

---

### **FASE 3: CAMBIOS EN MODELOS C#**

#### 3.1 Nuevo Modelo Sucursal
```csharp
public class Sucursal
{
    public int SucursalId { get; set; }
    public string Nombre { get; set; }
    public string? Codigo { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string Estado { get; set; }
    public DateTime FechaCreacion { get; set; }
}
```

#### 3.2 Modificar Modelos Existentes
Agregar `SucursalId` (nullable) a:
- `Usuario.cs`
- `Producto.cs`
- `Venta.cs`
- `Compra.cs`
- `Reserva.cs`
- `Caja.cs`
- `InventarioMovimiento.cs`

---

### **FASE 4: SERVICIOS Y LÓGICA DE NEGOCIO**

#### 4.1 Nuevo SucursalService
- [ ] Crear `ISucursalService` y `SucursalService`
- [ ] Métodos: Mostrar, Crear, Editar, Eliminar, ObtenerPorId
- [ ] Stored procedures: `usp_sucursales_*`

#### 4.2 Modificar Servicios Existentes

**Principio de Filtrado:**
- SuperAdmin: Sin filtro (ve todo)
- Contador: Sin filtro (ve todo)
- Admin/Vendedor: Filtrar por `SucursalId` del usuario

**Servicios a Modificar:**
- [ ] `ProductoService` - Filtrar productos por sucursal
- [ ] `VentaService` - Filtrar ventas por sucursal
- [ ] `CompraService` - Filtrar compras por sucursal
- [ ] `ReservaService` - Filtrar reservas por sucursal
- [ ] `CajaService` - Filtrar cajas por sucursal
- [ ] `StockService` - Calcular stock por sucursal
- [ ] `InventarioMovimientoService` - Filtrar movimientos por sucursal

#### 4.3 Helper para Obtener SucursalId del Usuario
Crear método helper que:
- Obtiene `SucursalId` de la sesión
- Si es SuperAdmin o Contador, retorna `null` (sin filtro)
- Si es Admin/Vendedor, retorna su `SucursalId`

---

### **FASE 5: MODIFICAR STORED PROCEDURES**

#### 5.1 Stored Procedures Nuevos
- [ ] `usp_sucursales_mostrar`
- [ ] `usp_sucursales_crear`
- [ ] `usp_sucursales_editar`
- [ ] `usp_sucursales_eliminar`
- [ ] `usp_sucursales_buscar`

#### 5.2 Stored Procedures a Modificar

**Agregar parámetro `@SucursalId` (opcional) a:**
- [ ] `usp_productos_mostrar` - Filtrar por sucursal
- [ ] `usp_ventas_mostrar` - Filtrar por sucursal
- [ ] `usp_compras_mostrar` - Filtrar por sucursal
- [ ] `usp_reservas_mostrar` - Filtrar por sucursal
- [ ] `usp_caja_mostrar` - Filtrar por sucursal
- [ ] `usp_inventario_movimientos_mostrar` - Filtrar por sucursal
- [ ] `usp_stock_actual_general` - Calcular por sucursal
- [ ] `usp_stock_actual_por_producto` - Calcular por sucursal

**Modificar stored procedures de creación para incluir SucursalId:**
- [ ] `usp_venta_completa` - Agregar `@SucursalId`
- [ ] `usp_compra_completa` - Agregar `@SucursalId`
- [ ] `usp_reserva_crear_completa` - Agregar `@SucursalId`
- [ ] `usp_caja_abrir` - Agregar `@SucursalId`
- [ ] `usp_productos_crear` - Agregar `@SucursalId`

---

### **FASE 6: CONTROLADORES Y VISTAS**

#### 6.1 Nuevo SucursalesController
- [ ] CRUD completo de sucursales
- [ ] Solo SuperAdmin puede crear/editar/eliminar
- [ ] Admin puede ver lista de sucursales (solo lectura)

#### 6.2 Modificar Controladores Existentes

**Agregar filtrado por sucursal en:**
- [ ] `ProductosController` - Filtrar productos
- [ ] `VentasController` - Filtrar ventas
- [ ] `ComprasController` - Filtrar compras
- [ ] `ReservasController` - Filtrar reservas
- [ ] `CajaController` - Filtrar cajas
- [ ] `MovimientosInventarioController` - Filtrar movimientos

**Modificar UsuariosController:**
- [ ] Agregar campo `SucursalId` en Create/Edit
- [ ] SuperAdmin puede asignar cualquier sucursal
- [ ] Admin solo puede asignar su propia sucursal

#### 6.3 Modificar Vistas

**Agregar selector de sucursal en:**
- [ ] Crear/Editar Producto
- [ ] Crear/Editar Venta
- [ ] Crear/Editar Compra
- [ ] Crear/Editar Reserva
- [ ] Abrir Caja

**Agregar filtro de sucursal en listados:**
- [ ] Lista de Productos (dropdown filtro)
- [ ] Lista de Ventas (dropdown filtro)
- [ ] Lista de Compras (dropdown filtro)
- [ ] Lista de Reservas (dropdown filtro)
- [ ] Lista de Cajas (dropdown filtro)

**SuperAdmin y Contador:**
- [ ] Ver selector "Todas las sucursales" o "Sucursal específica"
- [ ] Ver totales consolidados

---

### **FASE 7: AUTENTICACIÓN Y SESIÓN**

#### 7.1 Modificar LoginController
- [ ] Al hacer login, guardar `SucursalId` en sesión
- [ ] Si usuario es SuperAdmin/Contador, guardar `null` o `0`

#### 7.2 Modificar Sesión
Agregar a sesión:
```csharp
HttpContext.Session.SetInt32("SucursalId", usuario.SucursalId ?? 0);
HttpContext.Session.SetString("SucursalNombre", usuario.Sucursal?.Nombre ?? "Todas");
```

#### 7.3 Modificar GlobalAuthorizationFilter
- [ ] Verificar acceso según rol y sucursal
- [ ] SuperAdmin/Contador: Acceso completo
- [ ] Admin/Vendedor: Solo su sucursal

---

### **FASE 8: STOCK POR SUCURSAL**

#### 8.1 Concepto de Stock
- **Stock Físico por Sucursal**: Cantidad real en cada tienda
- **Stock Global**: Suma de todas las sucursales (solo SuperAdmin/Contador)

#### 8.2 Modificar Cálculo de Stock
- [ ] `StockService` debe calcular por `SucursalId`
- [ ] Movimientos de inventario deben incluir `SucursalId`
- [ ] Validaciones de stock deben ser por sucursal

#### 8.3 Transferencias entre Sucursales (Futuro)
- [ ] Tabla `tbl_transferencias` (opcional para fase futura)
- [ ] Tipo de movimiento `TRANSFERENCIA_SALIDA` y `TRANSFERENCIA_ENTRADA`

---

### **FASE 9: CAJA POR SUCURSAL**

#### 9.1 Modificar Sistema de Caja
- [ ] Cada sucursal tiene su propia caja
- [ ] Solo puede haber una caja abierta por sucursal
- [ ] Admin de sucursal solo ve su caja
- [ ] SuperAdmin/Contador ve todas las cajas

---

### **FASE 10: REPORTES Y DASHBOARD**

#### 10.1 Modificar Dashboard
- [ ] Mostrar métricas por sucursal
- [ ] SuperAdmin/Contador: Ver consolidado o por sucursal
- [ ] Admin/Vendedor: Ver solo su sucursal

#### 10.2 Reportes
- [ ] Reportes de ventas por sucursal
- [ ] Reportes de compras por sucursal
- [ ] Reportes consolidados (SuperAdmin/Contador)

---

## ⚠️ CONSIDERACIONES IMPORTANTES

### **1. Migración de Datos Existentes**
- Todos los datos actuales deben asignarse a una sucursal "Principal" o "Sucursal 1"
- Esto debe hacerse ANTES de permitir crear nuevas sucursales

### **2. Stock Inicial**
- Al crear una nueva sucursal, el stock inicial será 0
- Se necesitará un proceso de "Transferencia inicial" o "Ajuste inicial"

### **3. Clientes y Proveedores**
- **DECISIÓN**: ¿Clientes son globales o por sucursal?
  - **Recomendación**: Globales (un cliente puede comprar en cualquier sucursal)
- **DECISIÓN**: ¿Proveedores son globales o por sucursal?
  - **Recomendación**: Globales (se compra de los mismos proveedores)

### **4. Productos**
- **DECISIÓN**: ¿Un producto existe en todas las sucursales o se crea por sucursal?
  - **Recomendación**: Productos son globales, pero el STOCK es por sucursal
  - Un producto puede tener stock en Sucursal 1 pero no en Sucursal 2

### **5. Reservas**
- Las reservas son por sucursal
- Un cliente puede tener reservas en diferentes sucursales
- Al crear venta, solo se muestran reservas de la sucursal actual

### **6. Usuarios**
- SuperAdmin: `SucursalId = NULL` (ve todo)
- Contador: `SucursalId = NULL` (ve todo)
- Admin: `SucursalId = [su sucursal]` (ve solo su sucursal)
- Vendedor: `SucursalId = [su sucursal]` (ve solo su sucursal)

---

## 📝 ORDEN DE IMPLEMENTACIÓN RECOMENDADO

1. **FASE 1**: Preparación (Backup, análisis)
2. **FASE 2**: Base de datos (Tablas, columnas, constraints)
3. **FASE 3**: Modelos C# (Nuevos y modificados)
4. **FASE 4**: Servicios (SucursalService y modificaciones)
5. **FASE 5**: Stored Procedures (Nuevos y modificados)
6. **FASE 7**: Autenticación/Sesión (Para que el sistema sepa qué sucursal usar)
7. **FASE 6**: Controladores y Vistas (UI)
8. **FASE 8**: Stock por sucursal (Lógica crítica)
9. **FASE 9**: Caja por sucursal
10. **FASE 10**: Reportes y Dashboard

---

## 🧪 PRUEBAS NECESARIAS

### **Pruebas por Rol:**
- [ ] SuperAdmin puede ver todas las sucursales
- [ ] Admin solo ve su sucursal
- [ ] Vendedor solo ve su sucursal
- [ ] Contador puede ver todas las sucursales

### **Pruebas Funcionales:**
- [ ] Crear producto en Sucursal 1, no aparece en Sucursal 2
- [ ] Stock se calcula correctamente por sucursal
- [ ] Ventas se registran en la sucursal correcta
- [ ] Caja se abre por sucursal
- [ ] Reservas se filtran por sucursal

---

## 📋 CHECKLIST FINAL

Antes de comenzar, confirma:
- [ ] ¿Tienes backup de la base de datos?
- [ ] ¿Has definido los nombres exactos de los roles?
- [ ] ¿Has decidido qué sucursal asignar a los datos existentes?
- [ ] ¿Has identificado qué usuarios son SuperAdmin?
- [ ] ¿Necesitas crear el rol "Contador" o ya existe?
- [ ] ¿Estás listo para hacer cambios extensivos en el código?

---

## 🚀 SIGUIENTE PASO

Una vez que revises este plan y confirmes las decisiones, procederé a:
1. Crear los scripts SQL de migración
2. Crear los modelos C# necesarios
3. Modificar los servicios paso a paso
4. Actualizar las vistas

**¿Estás listo para comenzar o tienes preguntas sobre el plan?**
