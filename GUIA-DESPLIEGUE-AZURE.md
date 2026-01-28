# Guía de Despliegue y Actualización en Azure

## ✅ Respuesta Rápida

**SÍ**, puedes subir tu aplicación a Azure tal como está y seguir haciendo cambios sin problemas. Azure App Service está diseñado para esto.

## 🔄 Cómo Funciona el Flujo de Trabajo

### Opción 1: Despliegue Automático (Recomendado) 🚀

Con GitHub Actions configurado, cada vez que hagas `git push` a la rama `main` o `master`, tu aplicación se desplegará automáticamente.

**Pasos para configurar:**

1. **Crear App Service en Azure Portal:**
   - Ve a Azure Portal > App Services > Create
   - Nombre: `distribuidora-solares-app` (o el que prefieras)
   - Runtime: .NET 8.0
   - Plan: B1 (Basic) o superior

2. **Obtener el Publish Profile:**
   - En tu App Service > Overview > Get publish profile
   - Descarga el archivo `.PublishSettings`

3. **Configurar GitHub Secret:**
   - Ve a tu repositorio en GitHub > Settings > Secrets and variables > Actions
   - Click "New repository secret"
   - Name: `AZURE_WEBAPP_PUBLISH_PROFILE`
   - Value: Copia TODO el contenido del archivo `.PublishSettings` descargado
   - Click "Add secret"

4. **Actualizar el workflow (si cambiaste el nombre del App Service):**
   - Edita `.github/workflows/azure-deploy.yml`
   - Cambia `AZURE_WEBAPP_NAME` por el nombre real de tu App Service

5. **Hacer push a GitHub:**
   ```bash
   git add .
   git commit -m "Configurar CI/CD"
   git push origin main
   ```

### Opción 2: Despliegue Manual 📤

Puedes desplegar manualmente desde Visual Studio o usando Azure CLI:

**Desde Visual Studio:**
- Click derecho en el proyecto > Publish
- Selecciona Azure > Azure App Service
- Selecciona tu App Service y publica

**Desde Azure CLI:**
```bash
az webapp deploy --resource-group <tu-resource-group> --name <tu-app-service> --src-path ./publish
```

## 🔐 IMPORTANTE: Configurar Variables de Entorno

**CRÍTICO:** Tu `appsettings.json` contiene credenciales expuestas. Debes moverlas a Azure:

1. **En Azure Portal:**
   - Ve a tu App Service > Configuration > Application settings
   - Agrega estas variables:

   **Connection Strings:**
   - Name: `ConnectionStrings__DefaultConnection`
   - Value: `Server=TU_SERVIDOR.database.windows.net;Database=TU_BD;User Id=TU_USUARIO;Password=TU_PASSWORD;TrustServerCertificate=True;Encrypt=True;Connection Timeout=30;`
   - Type: SQLAzure

   - Name: `ConnectionStrings__AzureStorage`
   - Value: `DefaultEndpointsProtocol=https;AccountName=TU_CUENTA;AccountKey=TU_ACCOUNT_KEY_AQUI;EndpointSuffix=core.windows.net`
   - Type: Custom

   **Application Settings:**
   - Name: `AzureStorage__ContainerName`
   - Value: `productos-imagenes`

   - Name: `ASPNETCORE_ENVIRONMENT`
   - Value: `Production`

2. **Actualizar appsettings.json para desarrollo local:**
   - Mantén solo las configuraciones de desarrollo
   - Las credenciales de producción estarán en Azure

3. **Actualizar .gitignore:**
   - Asegúrate de que `appsettings.json` esté en `.gitignore` (o usa `appsettings.Development.json`)

## 📝 Flujo de Trabajo Diario

1. **Desarrollar localmente:**
   ```bash
   dotnet run
   ```

2. **Probar cambios localmente**

3. **Commit y Push:**
   ```bash
   git add .
   git commit -m "Descripción de los cambios"
   git push origin main
   ```

4. **Azure despliega automáticamente** (si usas GitHub Actions)

5. **Verificar en producción:**
   - Ve a tu URL de Azure App Service
   - Revisa los logs en Azure Portal > App Service > Log stream

## 🔍 Verificar Despliegue

- **Logs en tiempo real:**
  - Azure Portal > App Service > Log stream

- **Verificar estado:**
  - Azure Portal > App Service > Overview

- **Rollback si algo sale mal:**
  - Azure Portal > App Service > Deployment Center > History
  - Selecciona un despliegue anterior y haz "Redeploy"

## ⚠️ Consideraciones Importantes

1. **Base de Datos:**
   - Asegúrate de que Azure SQL Database tenga reglas de firewall que permitan conexiones desde tu App Service
   - Azure Portal > SQL Server > Networking > Add Azure services and resources

2. **Azure Storage:**
   - Verifica que el container `productos-imagenes` exista en tu cuenta de Storage

3. **Costo:**
   - El plan B1 tiene un costo mensual
   - Puedes usar el plan F1 (Free) para pruebas, pero tiene limitaciones

4. **Backups:**
   - Configura backups automáticos en Azure Portal > App Service > Backup

## 🆘 Solución de Problemas

**La app no se despliega:**
- Revisa los logs de GitHub Actions
- Verifica que el Publish Profile sea correcto
- Asegúrate de que las variables de entorno estén configuradas

**La app no conecta a la base de datos:**
- Verifica las reglas de firewall de Azure SQL
- Revisa que la connection string sea correcta en Azure Portal

**Los cambios no aparecen:**
- Espera 2-3 minutos después del push
- Verifica que el despliegue se completó en GitHub Actions
- Limpia la caché del navegador
