# Guía Paso a Paso: Crear Azure Blob Storage

## Paso 1: Acceder a Azure Portal

1. Ve a [https://portal.azure.com](https://portal.azure.com)
2. Inicia sesión con tu cuenta de Azure (la misma donde tienes tu base de datos)

## Paso 2: Crear Storage Account (Cuenta de Almacenamiento)

### Opción A: Desde el Menú Principal

1. En la página principal de Azure Portal, busca en la barra superior: **"Storage accounts"** o **"Cuentas de almacenamiento"**
2. Haz clic en **"Storage accounts"** en los resultados
3. Haz clic en el botón **"+ Create"** (o **"+ Crear"**)

### Opción B: Desde "Create a resource"

1. Haz clic en **"+ Create a resource"** (o **"+ Crear un recurso"**)
2. Busca **"Storage account"** o **"Cuenta de almacenamiento"**
3. Selecciona **"Storage account"** y haz clic en **"Create"** (o **"Crear"**)

## Paso 3: Configurar el Formulario de Creación

### Pestaña "Basics" (Básicos)

1. **Subscription** (Suscripción):
   - Selecciona la misma suscripción donde tienes tu base de datos

2. **Resource group** (Grupo de recursos):
   - **Recomendación**: Selecciona el mismo grupo de recursos donde tienes tu base de datos
   - O crea uno nuevo con un nombre descriptivo como: `rg-distribuidora-solares`

3. **Storage account name** (Nombre de la cuenta de almacenamiento):
   - **IMPORTANTE**: Debe ser único globalmente (solo letras minúsculas y números)
   - Ejemplos válidos:
     - `distribuidorasolares` (si está disponible)
     - `distribuidorasolares2026`
     - `distribuidorasolaresimg`
   - El sistema te avisará si el nombre está disponible (aparece un ✓ verde)

4. **Region** (Región):
   - Selecciona la misma región donde tienes tu base de datos para mejor rendimiento
   - Ejemplo: **Este de EE. UU.**, **Oeste de Europa**, **Centro de EE. UU.**, etc.

5. **Tipo de almacenamiento preferido** (Preferred storage type):
   - **IMPORTANTE**: Selecciona **"Azure Blob Storage o Azure Data Lake Storage Gen2"**
   - Esta es la opción correcta para almacenar imágenes y archivos binarios
   - Las otras opciones son para otros tipos de datos (archivos compartidos, tablas, colas)

6. **Performance** (Rendimiento):
   - Selecciona **Standard** (Estándar) - más económico y suficiente para imágenes
   - **NO selecciones Premium** (solo necesario para aplicaciones que requieren latencia muy baja)

7. **Redundancy** (Redundancia):
   - Para empezar, selecciona **LRS (Locally Redundant Storage)** - Almacenamiento con redundancia local
   - Es la opción más económica (~$0.018/GB/mes)
   - Si necesitas más redundancia después, puedes cambiarlo

8. Haz clic en **"Review"** (Revisar) o **"Next: Advanced"** (Siguiente: Avanzado)

### Pestaña "Advanced" (Avanzado) - Opcional pero Recomendado

**IMPORTANTE**: Aunque esta pestaña es opcional, hay configuraciones importantes aquí:

1. **Security** (Seguridad):
   - **Enable storage account key access**: Debe estar **habilitado** (por defecto)
     - Esto permite usar las claves de acceso para conectarte desde tu aplicación
   - **Allow Blob public access**: **Habilitado** (muy importante)
     - Esto permite que las imágenes sean accesibles públicamente vía URL
     - Sin esto, las imágenes no se podrán mostrar en tu catálogo web
   - **Minimum TLS version**: Puedes dejar el valor por defecto (TLS 1.2)

2. **Data protection** (Protección de datos):
   - **Enable versioning**: Puedes dejarlo deshabilitado por ahora (opcional)
   - **Enable soft delete for blobs**: Puedes dejarlo deshabilitado por ahora (opcional)
   - Estos son para recuperación de datos, no son necesarios para empezar

3. **Networking** (Redes) - Si aparece:
   - **Network access**: Deja **"Enable public access from all networks"** (por defecto)
     - Esto permite acceso público a las imágenes

4. Haz clic en **"Review"** (Revisar) o **"Next: Encryption"** (Siguiente: Cifrado)

### Pestaña "Encryption" (Cifrado) - Si aparece

- Puedes dejar todos los valores por defecto
- El cifrado está habilitado por defecto y es suficiente

### Pestaña "Tags" (Etiquetas) - Opcional

- Puedes saltarte esta pestaña o agregar etiquetas si quieres organizar tus recursos
- No es necesario para que funcione

3. Haz clic en **"Review"** (Revisar)

### Pestaña "Review" (Revisar)

1. Revisa todos los valores
2. Verifica que el costo estimado sea razonable
3. Haz clic en **"Create"** (Crear)

## Paso 4: Esperar la Creación

- El proceso toma aproximadamente **1-2 minutos**
- Verás una notificación cuando esté listo
- Haz clic en **"Go to resource"** (Ir al recurso) cuando aparezca

## Paso 5: Crear el Contenedor

Una vez creada la cuenta de Storage:

1. En el menú izquierdo de tu Storage Account, busca **"Data management"** (Administración de datos) > **"Containers"** (Contenedores)
2. Haz clic en **"+ Container"** (o **"+ Contenedor"**)
3. Configura:
   - **Name** (Nombre): `productos-imagenes` (o el nombre que prefieras)
   - **Public access level** (Nivel de acceso público): Selecciona **"Blob (anonymous read access for blobs only)"** 
     - En español: **"Blob (acceso de lectura anónimo solo para blobs)"**
     - Esto permite que las imágenes sean accesibles públicamente vía URL
4. Haz clic en **"Create"** (Crear)

## Paso 6: Obtener la Connection String (Cadena de Conexión)

1. En tu Storage Account, ve a **"Security + networking"** (Seguridad y redes) > **"Access keys"** (Claves de acceso)
2. Verás dos claves (key1 y key2) - puedes usar cualquiera
3. Haz clic en el icono de **"Show"** (mostrar) junto a **"key1"**
4. Copia la **"Connection string"** completa (no solo la clave)
   - Se ve así: `DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;EndpointSuffix=core.windows.net`

## Paso 7: Configurar en tu Aplicación

1. Abre `appsettings.json` en tu proyecto
2. Reemplaza la línea de `AzureStorage` con tu connection string real:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "...",
    "AzureStorage": "DefaultEndpointsProtocol=https;AccountName=TU_CUENTA_REAL;AccountKey=TU_CLAVE_REAL;EndpointSuffix=core.windows.net"
  },
  "AzureStorage": {
    "ContainerName": "productos-imagenes"
  }
}
```

## Paso 8: Probar la Configuración

1. Ejecuta tu aplicación: `dotnet run`
2. Ve a la sección de Productos
3. Selecciona un producto y haz clic en "Imágenes"
4. Intenta subir una imagen de prueba
5. Si funciona, verás la imagen en Azure Portal:
   - Ve a tu Storage Account > Containers > productos-imagenes
   - Deberías ver la imagen subida

## Verificación Rápida

### ¿Cómo saber si está bien configurado?

1. ✅ Storage Account creada y activa
2. ✅ Contenedor `productos-imagenes` creado con acceso público "Blob"
3. ✅ Connection string copiada correctamente en `appsettings.json`
4. ✅ Nombre del contenedor coincide en `appsettings.json`

## Solución de Problemas

### Error: "Storage account name not available" (Nombre de cuenta no disponible)
- El nombre ya está en uso
- Prueba con variaciones: `distribuidorasolares2`, `distribuidorasolares2026`, etc.

### Error: "AzureStorage connection string no configurada"
- Verifica que copiaste la connection string completa
- Verifica que está en `ConnectionStrings:AzureStorage` (no solo `AzureStorage`)

### Las imágenes no se muestran
- Verifica que el contenedor tenga acceso público "Blob"
- Verifica que la connection string sea correcta
- Revisa los logs de la aplicación para ver errores específicos

### Error al subir imágenes
- Verifica que el contenedor existe
- Verifica que tienes permisos en la cuenta de Storage
- Revisa que el tamaño del archivo no exceda 10 MB

## Costos Estimados

Con la configuración recomendada (LRS, Standard):
- **Primeros 50 GB**: ~$0.90/mes
- **Transferencia**: Primeros 5 GB gratis, luego ~$0.05/GB
- **Operaciones**: Muy económicas (~$0.0004 por 10,000 transacciones)

**Total estimado para empezar**: ~$1-2/mes

## Próximos Pasos

Una vez configurado:
1. Prueba subir una imagen desde la aplicación
2. Verifica que se guarde en Azure Portal
3. Verifica que la URL funcione en el navegador
4. Listo para usar en producción y en tu catálogo web

## Notas Importantes

- **Nombre único**: El nombre de la cuenta de Storage debe ser único en todo Azure
- **Acceso público**: El contenedor debe tener acceso público "Blob" para que las imágenes sean accesibles
- **Connection String**: Copia la cadena completa, no solo la clave
- **Región**: Usa la misma región que tu base de datos para mejor rendimiento
