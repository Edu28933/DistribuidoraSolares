# Guía Paso a Paso: Crear Azure Blob Storage (Español Completo)

## Paso 1: Entrar a Azure Portal

1. Ve a [https://portal.azure.com](https://portal.azure.com)
2. Inicia sesión con tu cuenta de Azure (la misma donde tienes tu base de datos)

## Paso 2: Crear la Cuenta de Almacenamiento

1. En la barra superior de Azure Portal, busca: **"Storage accounts"** o **"Cuentas de almacenamiento"**
2. Haz clic en **"Storage accounts"** en los resultados
3. Haz clic en el botón **"+ Create"** o **"+ Crear"**

## Paso 3: Llenar el Formulario - Pestaña "Basics" (Básicos)

### 1. Suscripción (Subscription)
- Selecciona la misma suscripción donde tienes tu base de datos

### 2. Grupo de Recursos (Resource group)
- Selecciona el mismo grupo de recursos donde tienes tu base de datos
- O crea uno nuevo con un nombre como: `rg-distribuidora-solares`

### 3. Nombre de la Cuenta de Almacenamiento (Storage account name)
- **MUY IMPORTANTE**: Debe ser único en todo Azure (solo letras minúsculas y números)
- Ejemplos que puedes probar:
  - `distribuidorasolares`
  - `distribuidorasolares2026`
  - `distribuidorasolaresimg`
- El sistema te dirá si está disponible (aparece un ✓ verde)

### 4. Región (Region)
- Selecciona la misma región donde tienes tu base de datos
- Ejemplos: **Este de EE. UU.**, **Oeste de Europa**, **Centro de EE. UU.**

### 5. Tipo de Almacenamiento Preferido (Preferred storage type)
- **SELECCIONA**: **"Azure Blob Storage o Azure Data Lake Storage Gen2"**
- Esta es la opción correcta para guardar imágenes

### 6. Rendimiento (Performance)
- Selecciona **"Standard"** (Estándar)
- **NO selecciones Premium** (es más caro y no lo necesitas)

### 7. Redundancia (Redundancy)
- Selecciona **"LRS (Locally Redundant Storage)"** - Almacenamiento con redundancia local
- Es la más económica (~$0.018 por GB al mes)

### 8. Continuar
- Haz clic en **"Next: Advanced"** (Siguiente: Avanzado) o **"Review"** (Revisar)

## Paso 4: Configurar la Pestaña "Advanced" (Avanzado)

### Sección "Security" (Seguridad)

1. **Requerir transferencia segura para las operaciones de API de REST** (Require secure transfer for REST API operations):
   - Debe estar **marcado** (habilitado) - ya viene así por defecto
   - Esto asegura que todas las conexiones sean seguras (HTTPS)

2. **Permitir el acceso anónimo en contenedores individuales** (Allow anonymous access in individual containers):
   - **DEBE estar MARCADO** (habilitado) - **MUY IMPORTANTE**
   - Esta opción permite que los contenedores individuales puedan tener acceso público
   - Sin esto, aunque crees un contenedor con acceso público, las imágenes no se mostrarán
   - **Si está desmarcado, márcalo ahora**

3. **Habilitar el acceso a la clave de la cuenta de almacenamiento** (Enable storage account key access):
   - Debe estar **marcado** (habilitado) - ya viene así por defecto
   - Esto permite que tu aplicación se conecte usando las claves de acceso

4. **El valor predeterminado es la autorización de Microsoft Entra en Azure Portal**:
   - Déjalo **desmarcado** (no lo necesitas)

5. **Versión mínima de TLS** (Minimum TLS version):
   - Déjalo como está: **"Versión 1.2"** (es el valor por defecto y es correcto)

6. **Ámbito permitido para las operaciones de copia** (Allowed scope for copy operations):
   - Déjalo como está: **"Desde cualquier cuenta de almacenamiento"** (From any storage account)

### Sección "Almacenamiento de blobs" (Blob storage)

1. **Permitir replicación entre inquilinos** (Allow cross-tenant replication):
   - Déjalo **desmarcado** (no lo necesitas)

2. **Nivel de acceso** (Access tier):
   - **SELECCIONA**: **"Frecuente" (Hot)**
   - Esta es la opción correcta para imágenes que se acceden frecuentemente (como en un catálogo web)
   - Descripción: "Optimizado para escenarios de uso diario y datos a los que se accede con frecuencia"
   - **NO selecciones "Esporádico" o "Acceso esporádico"** (son para datos que se acceden raramente)

### Sección "Espacio de nombres jerárquico" (Hierarchical namespace)

- **Habilitar el espacio de nombres jerárquico**: Déjalo **desmarcado**
- No lo necesitas para almacenar imágenes simples

### Sección "Protocolos de acceso" (Access protocols)

- **Habilitar SFTP**: Déjalo **desmarcado**
- **Habilitar el sistema de archivos de red v3**: Déjalo **desmarcado**
- Estas opciones son para casos especiales, no las necesitas

### Sección "Azure Files"

- **Habilitar recursos compartidos de archivos grandes**: Puedes dejarlo marcado o desmarcado (no afecta el Blob Storage)
- **Habilitar la identidad administrada de SMB**: Déjalo desmarcado

### Continuar
- Haz clic en **"Siguiente"** (Next) o **"Revisar y crear"** (Review and create)

## Paso 5: Otras Pestañas (Si Aparecen)

### Pestaña "Encryption" (Cifrado)
- Déjalo todo como está (valores por defecto)
- El cifrado ya viene habilitado

### Pestaña "Tags" (Etiquetas)
- Puedes saltártela
- No es necesaria

## Paso 6: Revisar y Crear

1. Haz clic en **"Review"** (Revisar)
2. Revisa todos los valores que ingresaste
3. Verifica el costo estimado (debería ser bajo, ~$1-2/mes)
4. Haz clic en **"Create"** (Crear)

## Paso 7: Esperar la Creación

- El proceso toma **1-2 minutos**
- Verás una notificación cuando termine
- Haz clic en **"Go to resource"** (Ir al recurso) cuando aparezca

## Paso 8: Crear el Contenedor (OPCIONAL - El código lo crea automáticamente)

**IMPORTANTE:** Tu código ya está configurado para crear el contenedor automáticamente cuando la aplicación se ejecute. **NO necesitas crearlo manualmente.**

Sin embargo, si quieres crearlo manualmente o verificar que existe:

1. En el menú izquierdo, busca **"Data storage"** (Almacenamiento de datos) - **NO "Data management"**
2. Haz clic en **"Containers"** (Contenedores)
3. Si no existe, haz clic en **"+ Container"** o **"+ Contenedor"**
4. Configura:
   - **Name** (Nombre): Escribe `productos-imagenes`
   - **Public access level** (Nivel de acceso público): 
     - Selecciona **"Blob (anonymous read access for blobs only)"**
     - En español: **"Blob (acceso de lectura anónimo solo para blobs)"**
5. Haz clic en **"Create"** (Crear)

**Nota:** Si no encuentras "Containers" en el menú, no te preocupes. El código lo creará automáticamente cuando ejecutes la aplicación.

## Paso 9: Obtener la Cadena de Conexión (Connection String)

1. En el menú izquierdo de tu Storage Account, busca **"Security + networking"** (Seguridad y redes)
2. Haz clic en **"Access keys"** (Claves de acceso)
3. Verás dos claves (key1 y key2) - puedes usar cualquiera
4. Junto a **"key1"**, haz clic en el icono de **"Show"** (mostrar - icono de ojo)
5. Copia la **"Connection string"** completa (no solo la clave)
   - Se ve así: `DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;EndpointSuffix=core.windows.net`

## Paso 10: Configurar en tu Aplicación

1. Abre el archivo `appsettings.json` en tu proyecto
2. Busca la línea que dice `"AzureStorage": "DefaultEndpointsProtocol=https;AccountName=TU_CUENTA_STORAGE..."`
3. Reemplázala con la Connection String que copiaste:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "...",
    "AzureStorage": "PEGA_AQUI_LA_CONNECTION_STRING_QUE_COPIASTE"
  },
  "AzureStorage": {
    "ContainerName": "productos-imagenes"
  }
}
```

## Paso 11: Probar que Funcione

1. Abre una terminal en tu proyecto
2. Ejecuta: `dotnet restore` (para instalar el paquete Azure.Storage.Blobs)
3. Ejecuta: `dotnet run`
4. Ve a tu aplicación en el navegador
5. Ve a **Productos** > Selecciona un producto > Haz clic en **"Imágenes"**
6. Sube una imagen de prueba
7. Si funciona, verás la imagen en Azure Portal:
   - Ve a tu Storage Account > Containers > productos-imagenes
   - Deberías ver la imagen que subiste

## Checklist de Verificación

Antes de probar, verifica que tengas:

- [ ] Storage Account creada y activa
- [ ] Contenedor `productos-imagenes` creado
- [ ] El contenedor tiene acceso público **"Blob"**
- [ ] Connection string copiada en `appsettings.json`
- [ ] El nombre del contenedor en `appsettings.json` es `productos-imagenes`

## Solución de Problemas Comunes

### Error: "Storage account name not available"
- El nombre ya está en uso
- Prueba con otro: `distribuidorasolares2`, `distribuidorasolares2026`, etc.

### Error: "AzureStorage connection string no configurada"
- Verifica que copiaste la connection string completa
- Debe estar en `ConnectionStrings:AzureStorage` (con los dos puntos)

### Las imágenes no se muestran
- Verifica que el contenedor tenga acceso público **"Blob"**
- Verifica que la connection string sea correcta
- Revisa la consola de la aplicación para ver errores

### Error al subir imágenes
- Verifica que el contenedor existe en Azure Portal
- Verifica que el tamaño del archivo no exceda 10 MB
- Verifica que el formato sea JPG, PNG, GIF o WEBP

## Costos Estimados

Con la configuración que recomendamos (LRS, Standard):
- **Primeros 50 GB de imágenes**: ~$0.90/mes
- **Transferencia de datos**: Primeros 5 GB gratis, luego ~$0.05 por GB
- **Operaciones**: Muy baratas (~$0.0004 por 10,000 operaciones)

**Total estimado para empezar**: ~$1-2 dólares al mes

## Resumen de Configuración Importante

**En la pestaña "Basics":**
- Tipo de almacenamiento: **Azure Blob Storage o Azure Data Lake Storage Gen2**
- Rendimiento: **Standard**
- Redundancia: **LRS**

**En la pestaña "Advanced":**
- **Allow Blob public access**: **HABILITADO** (muy importante)

**Después de crear:**
- Contenedor: `productos-imagenes` con acceso público **"Blob"**

¡Listo! Con estos pasos deberías tener tu Azure Blob Storage configurado correctamente.
