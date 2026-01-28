# Azure App Service Deployment Configuration

## Configuración de Variables de Entorno

En Azure Portal, configure las siguientes variables de entorno en Configuration > Application Settings:

### Connection Strings
- `ConnectionStrings__DefaultConnection`: Cadena de conexión a Azure SQL Database

### Application Settings (Opcional)
- `ASPNETCORE_ENVIRONMENT`: `Production`
- `ASPNETCORE_URLS`: `http://+:80`

## Pasos para Desplegar

1. **Crear App Service en Azure Portal:**
   - Crear un nuevo App Service Plan (Linux o Windows)
   - Crear un nuevo Web App
   - Seleccionar .NET 8.0 como runtime stack

2. **Configurar Deployment Center:**
   - Conectar con GitHub
   - Seleccionar el repositorio y branch
   - Configurar build automático

3. **Configurar Connection String:**
   - En Configuration > Connection strings
   - Agregar `DefaultConnection` con la cadena de conexión a Azure SQL Database

4. **Configurar Firewall de Azure SQL:**
   - En Azure SQL Database > Networking
   - Agregar la IP del App Service a las reglas de firewall

## GitHub Actions (Opcional)

Puede configurar GitHub Actions para despliegue automático creando `.github/workflows/azure-deploy.yml`