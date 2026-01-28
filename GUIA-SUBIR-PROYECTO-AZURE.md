# Guía: Subir Distribuidora Solares a Azure

Pasos para publicar tu proyecto en Azure App Service y poder verlo desde cualquier parte en la web.

---

## Qué necesitas antes

1. **Cuenta de Azure** – [azure.microsoft.com](https://azure.microsoft.com) → Crear cuenta (hay nivel gratis de prueba).
2. **Proyecto en GitHub** – El código de DistribuidoraSolares en un repo (para despliegue automático).
3. **Base de datos en Azure SQL** – Puede ser una ya creada o una nueva; la app se conecta por connection string.

---

## Resumen rápido

| Paso | Qué haces |
|------|------------|
| 1 | Crear o usar un **grupo de recursos** en Azure |
| 2 | Tener (o crear) **Azure SQL Database** y la base `bd_distribuidora_solares` |
| 3 | Crear **App Service** (plan + web app) con runtime .NET 8 |
| 4 | Configurar en la app la **connection string** y, si usas fotos, **Azure Storage** |
| 5 | Abrir el **firewall** de SQL para que la App Service pueda conectarse |
| 6 | **Publicar** la app (manual o con GitHub Actions) |

---

## Paso 1: Grupo de recursos

1. Entra en [portal.azure.com](https://portal.azure.com).
2. **Grupos de recursos** → **Crear**.
3. Nombre: por ejemplo `rg-distribuidora-solares`.
4. Región: la que prefieras (ej. Este de EE. UU., Centro de México, etc.).
5. Crear.

---

## Paso 2: Base de datos (Azure SQL)

Si ya tienes un **servidor de Azure SQL** y la base `bd_distribuidora_solares`:

- Anota: **servidor** (`tu-servidor.database.windows.net`), **base de datos**, **usuario** y **contraseña**.
- Más adelante usarás esto en la connection string de la App Service.

Si aún no tienes base en Azure:

1. En el portal: **Bases de datos SQL** → **Crear**.
2. Elegir la suscripción y el **grupo de recursos** del paso 1.
3. Nombre de la base: `bd_distribuidora_solares`.
4. Crear un **servidor** nuevo si no tienes (nombre, usuario admin, contraseña, región).
5. Nivel de cómputo: por ejemplo **Básico** (5 DTU) para empezar.
6. Crear.
7. Cuando esté creada, en **Consultas** (o con SSMS conectado a ese servidor) ejecuta tus scripts SQL para crear tablas, stored procedures, etc. (los que usas en local, adaptados a esa base).

Tu **connection string** tendrá esta forma (sustituye por tus datos):

```
Server=TU_SERVIDOR.database.windows.net;Database=bd_distribuidora_solares;User Id=TU_USUARIO;Password=TU_CONTRASEÑA;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

---

## Paso 3: App Service (la app web)

1. En Azure Portal: **App Services** → **Crear** → **Aplicación web**.
2. **Suscripción** y **grupo de recursos** (el mismo de arriba).
3. **Nombre**: por ejemplo `distribuidora-solares-app` (será parte de la URL: `https://distribuidora-solares-app.azurewebsites.net`).
4. **Publicar**: Código.
5. **Stack en tiempo de ejecución**: .NET 8 (o .NET 6 LTS si lo usas).
6. **Sistema operativo**: Windows o Linux, según tu preferencia.
7. **Región**: la misma del grupo de recursos (o la más cercana).
8. **Plan de App Service**: crear uno nuevo; para pruebas puede ser **F1 (Gratis)** o **B1 (Básico)** si quieres más recursos.
9. Revisar y crear; esperar a que termine el despliegue.

---

## Paso 4: Configurar la aplicación en Azure

La app lee la connection string y la configuración desde **Configuration** del App Service. No dejes contraseñas en el código ni en el repo.

1. En el portal, abre tu **App Service**.
2. Menú **Configuración** → **Configuración de la aplicación** (Application settings).
3. **Nueva cadena de conexión**:
   - **Nombre**: `ConnectionStrings__DefaultConnection`
   - **Valor**: la connection string de Azure SQL del paso 2.
   - **Tipo**: SQL Azure.
4. Si la app usa **Azure Blob Storage** (fotos de productos):
   - **Nueva cadena de conexión**:
     - **Nombre**: `ConnectionStrings__AzureStorage`
     - **Valor**: la cadena de conexión de tu cuenta de Storage.
   - **Nueva configuración de la aplicación**:
     - **Nombre**: `AzureStorage__ContainerName`
     - **Valor**: `productos-imagenes` (o el nombre del contenedor que uses).
5. **Nueva configuración de la aplicación**:
   - **Nombre**: `ASPNETCORE_ENVIRONMENT`
   - **Valor**: `Production`
6. Guardar los cambios.

Tu `appsettings.json` local puede seguir teniendo *solo* la config de desarrollo. En Azure no hace falta que estén ahí las cadenas de producción; se sobrescriben con lo que pongas en Configuración.

---

## Paso 5: Firewall de Azure SQL

Para que la App Service pueda conectarse a la base:

1. En el portal, entra en el **servidor** de Azure SQL (no en la base de datos).
2. **Seguridad** → **Redes** (o **Networking**).
3. En **Reglas de firewall**:
   - Activa **Permitir que los servicios y recursos de Azure accedan a este servidor**.
4. Si quieres restringir por IP, puedes añadir la IP “outbound” de la App Service (se puede ver en las propiedades de la app o en los logs), pero para empezar suele bastar la opción anterior.
5. Guardar.

---

## Paso 6: Publicar la app

### Opción A: Publicar desde Visual Studio (manual)

1. Abre la solución en Visual Studio.
2. Clic derecho en el proyecto **DistribuidoraSolares** → **Publicar**.
3. Destino: **Azure** → **Azure App Service (Windows)** o **(Linux)** según lo que hayas elegido.
4. Iniciar sesión en Azure si te lo pide.
5. Elige la **suscripción**, el **grupo de recursos** y el **App Service** que creaste.
6. Publicar y esperar a que termine.

Cuando acabe, abre la URL del App Service (ej. `https://distribuidora-solares-app.azurewebsites.net`). Deberías ver la app (pantalla de login o la que tengas por defecto).

### Opción B: Despliegue automático con GitHub Actions

Si el código está en GitHub:

1. En tu App Service, **Centro de implementación** (o **Deployment Center**):
   - Origen: **GitHub**.
   - Autoriza Azure con tu cuenta de GitHub y elige **organización**, **repositorio** y **rama** (por ejemplo `main`).
   - Confirmar. Azure creará un workflow que despliega al hacer push.

O usar el workflow que ya tienes en el repo:

2. **Publish profile** del App Service:
   - En el App Service → **Descargar perfil de publicación** (Get publish profile).
   - Abre el `.PublishSettings` y copia **todo** el contenido.

3. En GitHub: **Tu repo** → **Settings** → **Secrets and variables** → **Actions**:
   - **New repository secret**.
   - **Name**: `AZURE_WEBAPP_PUBLISH_PROFILE`
   - **Value**: pega el contenido completo del perfil de publicación.
   - Guardar.

4. En el repo, abrir `.github/workflows/azure-deploy.yml` y comprobar que el nombre de la app coincide con tu App Service:
   - `AZURE_WEBAPP_NAME: distribuidora-solares-app` (o el nombre que hayas puesto).

5. Haz push a la rama que use el workflow (ej. `main`):
   ```bash
   git add .
   git commit -m "Preparar despliegue en Azure"
   git push origin main
   ```
6. En GitHub → pestaña **Actions** verás el workflow; cuando termine en verde, la app estará actualizada en la URL de Azure.

---

## Después de subir

- **URL**: `https://<nombre-de-tu-app>.azurewebsites.net`
- **Logs**: En Azure Portal → tu App Service → **Registro (Log stream)** o **Registros de App Service**.
- **Errores de conexión a base de datos**: Revisa connection string en Configuración y reglas de firewall del servidor SQL.
- **Cambios futuros**:  
  - Si usas despliegue con GitHub: push a la rama configurada.  
  - Si publicas manual: volver a publicar desde Visual Studio.

---

## Costes aproximados (orientativo)

- **App Service**
  - Plan **F1 (Gratis)**: suficiente para probar; limitaciones de rendimiento y tiempo de ejecución.
  - Plan **B1 (Básico)**: más estable para uso real; tiene costo mensual.
- **Azure SQL**
  - Nivel **Básico (5 DTU)**: bajo costo, adecuado para pocos usuarios.
- Revisa siempre la sección **Costes** en cada recurso en el portal para tu suscripción y región.

---

## Resumen de archivos útiles en tu repo

| Archivo | Uso |
|--------|-----|
| `GUIA-SUBIR-PROYECTO-AZURE.md` | Esta guía |
| `GUIA-DESPLIEGUE-AZURE.md` | Detalles de despliegue y variables |
| `GUIA-LIMPIAR-BASE-DATOS.md` | Cómo limpiar la base (también aplica a la de Azure si la usas para pruebas) |
| `.github/workflows/azure-deploy.yml` | Workflow de GitHub Actions para publicar en Azure |

Si sigues estos pasos en orden, podrás tener el proyecto en Azure y acceder a él desde cualquier lugar por la URL del App Service.
