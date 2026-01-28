# Guía para limpiar la base de datos (bd_distribuidora_solares)

**Usa esta guía cuando quieras borrar datos y dejar la base limpia.** No ejecutes nada sin leer antes y sin hacer backup.

---

## 1. Antes de tocar nada: hacer backup

Siempre haz una copia de seguridad antes de borrar datos.

### En SQL Server Management Studio
1. Clic derecho en la base **bd_distribuidora_solares**
2. **Tareas** → **Crear copia de seguridad...**
3. Tipo: **Completa**
4. Destino: por ejemplo `C:\Backups\bd_distribuidora_solares_YYYYMMDD.bak`
5. Aceptar y esperar a que termine

### Por comando (sqlcmd o PowerShell)
```sql
BACKUP DATABASE [bd_distribuidora_solares]
TO DISK = 'C:\Backups\bd_distribuidora_solares.bak'
WITH FORMAT, INIT, NAME = 'Backup antes de limpiar';
```

Si algo sale mal, podrás restaurar desde ese `.bak`.

---

## 2. Dos formas de “dejarla limpia”

### Opción A: Vaciar TODO
Borra datos de **todas** las tablas. La base queda vacía: sin usuarios, productos, ventas, nada. Tendrás que volver a crear roles, usuarios, categorías, productos, sucursales, etc. (o restaurar desde backup).

**Cuándo usarla:** cuando quieres “empezar de cero” o clonar un entorno de prueba limpio.

### Opción B: Vaciar solo datos operativos
Borra solo lo que se mueve en el día a día:
- Ventas y detalle
- Compras y detalle
- Movimientos de inventario
- Reservas y detalle
- Caja y movimientos de caja
- Permisos por pantalla (se pueden volver a asignar)

**Se mantiene:** categorías, productos, clientes, proveedores, usuarios, roles, sucursales, métodos de pago, pantallas.

**Cuándo usarla:** cuando quieres seguir usando los mismos catálogos y usuarios pero sin historial de ventas, compras, caja, etc.

---

## 3. Cómo ejecutar el script

En la raíz del proyecto está el archivo **`limpiar-base-datos.sql`**.

1. Abre **SQL Server Management Studio** (o tu cliente SQL).
2. Conecta al servidor donde está **bd_distribuidora_solares**.
3. Abre `limpiar-base-datos.sql`.
4. Al inicio del script verás una variable o secciones comentadas:
   - **Opción A:** descomenta o activa la parte “VACIAR TODO”.
   - **Opción B:** descomenta o activa la parte “VACIAR SOLO DATOS OPERATIVOS”.
5. Revisa qué bloque queda activo y ejecuta el script.
6. No ejecutes las dos opciones seguidas sin necesidad; elige una según lo que busques.

---

## 4. Orden de borrado (por si quieres armarlo a mano)

Las tablas tienen foreign keys, por eso hay que borrar primero las que dependen de otras.

### Para “Vaciar todo” (orden sugerido)
1. `tbl_venta_detalle`
2. `tbl_compra_detalle`
3. `tbl_reserva_detalle`
4. `tbl_inventario_movimientos`
5. `tbl_caja_movimientos`
6. `tbl_permisos_pantalla`
7. `tbl_usuario_emails`
8. `tbl_producto_fotos`
9. `tbl_ventas`
10. `tbl_compras`
11. `tbl_reservas`
12. `tbl_caja`
13. `tbl_usuarios`
14. `tbl_productos`
15. `tbl_clientes`
16. `tbl_proveedores`
17. `tbl_metodos_pago`
18. `tbl_sucursales`
19. `tbl_roles`
20. `tbl_categorias`
21. `tbl_pantallas` (si quieres vaciarla también; cuidado con permisos)

### Para “Solo datos operativos”
Borrar en este orden:
1. `tbl_venta_detalle`
2. `tbl_compra_detalle`
3. `tbl_reserva_detalle`
4. `tbl_inventario_movimientos`
5. `tbl_caja_movimientos`
6. `tbl_permisos_pantalla`
7. `tbl_ventas`
8. `tbl_compras`
9. `tbl_reservas`
10. `tbl_caja`

No se tocan: `tbl_categorias`, `tbl_productos`, `tbl_clientes`, `tbl_proveedores`, `tbl_usuarios`, `tbl_roles`, `tbl_sucursales`, `tbl_metodos_pago`, `tbl_pantallas`, `tbl_usuario_emails`, `tbl_producto_fotos`.

---

## 5. Después de limpiar

- Si vaciaste **todo**: tendrás que volver a ejecutar los scripts de creación/seed (roles, pantallas, usuario inicial, etc.) o restaurar desde backup y repetir solo la parte que quieras “limpiar”.
- Si vaciaste **solo datos operativos**: pantallas y permisos por pantalla habrán quedado vacíos o borrados según el script; si usas permisos, vuelve a ejecutar `insertar-todas-pantallas-sistema.sql` y asigna de nuevo los permisos por rol.

---

## 6. Resumen rápido

| Quiero…                         | Hacer backup | Luego ejecutar                    |
|---------------------------------|-------------|-----------------------------------|
| Dejarla vacía del todo          | Sí          | `limpiar-base-datos.sql` opción A |
| Solo quitar ventas/compras/caja | Sí          | `limpiar-base-datos.sql` opción B |

Siempre que dudes, haz backup y prueba primero en una copia de la base.
