# Configuración de Azure Blob Storage para Imágenes de Productos

## Pasos para Configurar Azure Blob Storage

### 1. Crear una Cuenta de Azure Storage

1. Inicia sesión en [Azure Portal](https://portal.azure.com)
2. Busca "Storage accounts" y haz clic en "Create"
3. Completa el formulario:
   - **Subscription**: Tu suscripción de Azure
   - **Resource group**: Crea uno nuevo o usa existente
   - **Storage account name**: Ejemplo: `distribuidorasolares` (debe ser único globalmente)
   - **Region**: Selecciona la región más cercana (ej: East US, West Europe)
   - **Performance**: Standard
   - **Redundancy**: LRS (Locally Redundant Storage) para empezar (más económico)
4. Haz clic en "Review + create" y luego "Create"
5. Espera a que se cree la cuenta (1-2 minutos)

### 2. Obtener la Cadena de Conexión

1. Ve a tu cuenta de Storage recién creada
2. En el menú izquierdo, busca "Security + networking" > "Access keys"
3. Haz clic en "Show" junto a "key1"
4. Copia la "Connection string" completa

### 3. Configurar el Contenedor

1. En tu cuenta de Storage, ve a "Data management" > "Containers"
2. Haz clic en "+ Container"
3. Configura:
   - **Name**: `productos-imagenes` (o el nombre que prefieras)
   - **Public access level**: **Blob** (para acceso público a las imágenes)
4. Haz clic en "Create"

### 4. Configurar la Aplicación

1. Abre `appsettings.json` en tu proyecto
2. Reemplaza la cadena de conexión en `AzureStorage`:
   ```json
   "ConnectionStrings": {
     "AzureStorage": "DefaultEndpointsProtocol=https;AccountName=TU_CUENTA;AccountKey=TU_CLAVE;EndpointSuffix=core.windows.net"
   }
   ```
3. Si quieres cambiar el nombre del contenedor:
   ```json
   "AzureStorage": {
     "ContainerName": "productos-imagenes"
   }
   ```

### 5. Instalar el Paquete NuGet

El paquete `Azure.Storage.Blobs` ya está agregado al proyecto. Si necesitas instalarlo manualmente:

```bash
dotnet add package Azure.Storage.Blobs
```

### 6. Configurar Variables de Entorno (Opcional pero Recomendado)

Para producción, usa Azure App Service Configuration:

1. En Azure Portal, ve a tu App Service
2. Ve a "Configuration" > "Application settings"
3. Agrega:
   - **Name**: `ConnectionStrings__AzureStorage`
   - **Value**: Tu connection string completa
   - **Name**: `AzureStorage__ContainerName`
   - **Value**: `productos-imagenes`

## Estructura de Archivos en Azure Blob Storage

Las imágenes se organizan así:
```
productos-imagenes/
├── producto-1/
│   ├── guid1.jpg
│   ├── guid2.png
│   └── guid3.webp
├── producto-5/
│   ├── guid4.jpg
│   └── guid5.png
└── ...
```

## URLs de las Imágenes

Las imágenes se almacenan con URLs públicas como:
```
https://TU_CUENTA.blob.core.windows.net/productos-imagenes/producto-1/guid1.jpg
```

Estas URLs se guardan en la tabla `Productos_Fotos` en el campo `UrlFoto`.

## Funcionalidades Implementadas

✅ **Subir múltiples imágenes por producto**
- Cada producto puede tener múltiples imágenes
- Formato: `producto-{ProductoId}/{GUID}.{extension}`

✅ **Seleccionar imagen principal**
- Solo una imagen puede ser principal por producto
- Al marcar una como principal, las demás se desmarcan automáticamente

✅ **Agregar más imágenes**
- Usa el formulario de subida en la página de imágenes
- Puedes marcar como principal al subir

✅ **Eliminar imágenes**
- Elimina tanto el archivo de Azure como el registro en BD
- Confirmación antes de eliminar

## Costos Estimados

Para una cuenta de Storage con:
- **Hot tier** (acceso frecuente)
- **LRS** (redundancia local)
- **Región**: US East

**Ejemplo mensual:**
- 20 GB de imágenes: ~$0.36/mes
- Transferencia (15 GB): ~$0.75/mes
- **Total**: ~$1.11/mes

## Migración de Imágenes Existentes

Si ya tienes imágenes en `wwwroot/images/productos/`, puedes crear un script de migración para subirlas a Azure Blob Storage.

## Troubleshooting

### Error: "AzureStorage connection string no configurada"
- Verifica que `appsettings.json` tenga la cadena de conexión correcta
- O configura las variables de entorno en Azure App Service

### Error: "Container not found"
- Verifica que el contenedor exista en Azure Portal
- Verifica que el nombre del contenedor en `appsettings.json` coincida

### Las imágenes no se muestran
- Verifica que el contenedor tenga acceso público "Blob"
- Verifica que las URLs en la BD sean correctas
- Revisa la consola del navegador para errores de CORS

## Seguridad

- Las imágenes son públicas (acceso Blob)
- Para imágenes privadas, cambia a acceso privado y usa SAS tokens
- Considera usar Azure CDN para mejor rendimiento en producción
