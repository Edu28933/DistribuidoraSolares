# Distribuidora Solares

Sistema de gestión comercial para distribuidora de productos solares.

## Características

- Gestión de productos y categorías
- Control de inventario (Kardex)
- Punto de venta (POS)
- Gestión de compras
- Sistema de reservas
- Control de caja
- Gestión de clientes y proveedores
- Reportes y consultas

## Tecnologías

- .NET 8.0
- ASP.NET Core MVC
- Entity Framework Core
- SQL Server / Azure SQL Database
- Bootstrap 5

## Configuración

1. Clonar el repositorio
2. Configurar la cadena de conexión en `appsettings.json`
3. Ejecutar `dotnet restore`
4. Ejecutar `dotnet run`

## Base de Datos

La aplicación utiliza stored procedures para todas las operaciones de base de datos. Asegúrese de tener ejecutado el script de creación de la base de datos.

## Despliegue en Azure

La aplicación está configurada para desplegarse en Azure App Service. Configure las variables de entorno en Azure Portal:

- `ConnectionStrings__DefaultConnection`: Cadena de conexión a Azure SQL Database

## Licencia

Copyright © 2026