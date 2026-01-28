# Guía: Subir el proyecto a GitHub

Pasos para publicar **Distribuidora Solares** en GitHub y tener el código en la nube.

---

## SOLUCIÓN INMEDIATA – "Error al insertar en el repositorio remoto"

Si estás en la rama **subir** con commits pendientes y te sale ese error, haz esto **en este orden**:

### 1. Crear un Personal Access Token (PAT) en GitHub

1. Abre: **https://github.com/settings/tokens**
2. **Generate new token** → **Generate new token (classic)**.
3. **Note**: `DistribuidoraSolares` (o el nombre que quieras).
4. **Expiration**: 90 days o No expiration.
5. Marca el permiso **repo**.
6. **Generate token** → **cópialo** y guárdalo en un lugar seguro. **Solo se muestra una vez.**

### 2. Borrar credenciales viejas de GitHub en Windows

1. **Panel de control** → **Administrador de credenciales** (o busca "Credenciales de Windows" en el menú inicio).
2. **Credenciales de Windows**.
3. Busca entradas que digan `github.com` o `git:https://github.com`.
4. **Eliminar** cada una para que la próxima vez Git te pida usuario y contraseña de nuevo.

### 3. Hacer el push desde PowerShell (con el PAT)

Abre **PowerShell** y ejecuta **tal cual** (tu rama actual es **subir**):

```powershell
cd "c:\Users\eduar\source\repos\DistribuidoraSolares"
git push -u origin subir
```

Cuando pida:
- **Username for 'https://github.com':** escribe `Edu28933`
- **Password for 'https://Edu28933@github.com':** **pega el PAT** (no la contraseña de tu cuenta de GitHub).

Si todo va bien, los 2 commits de la rama **subir** se subirán al repo.

### 4. (Opcional) Dejar todo en la rama `main` en GitHub

Si quieres que en GitHub la rama principal sea `main`:

```powershell
git checkout master
git merge subir
git branch -M main
git push -u origin main
```

Cuando pida contraseña, vuelve a usar el **PAT**.

---

## Antes de empezar

1. **Cuenta en GitHub** – [github.com](https://github.com) → Sign up si no tienes.
2. **Git instalado** – En Windows suele venir con Visual Studio, o descarga: [git-scm.com](https://git-scm.com).

Para comprobar que Git está instalado, abre **PowerShell** o **Terminal** y escribe:

```bash
git --version
```

---

## Paso 1: No subir credenciales

Tu proyecto tiene **`appsettings.json`** con cadenas de conexión y claves. Ese archivo **no debe** subirse a GitHub.

En el **`.gitignore`** ya están ignorados:

- `**/appsettings.json`
- `**/appsettings.Development.json`

Así, aunque hagas `git add .`, esos archivos no se incluirán. En el repo sí puedes tener **`appsettings.example.json`** como plantilla (sin datos reales).

Cada quien debe copiar la plantilla a `appsettings.json` y rellenar sus propios valores en su máquina.

---

## Paso 2: Inicializar Git en el proyecto

Abre **PowerShell** o **Terminal** y ve a la carpeta del proyecto:

```bash
cd "c:\Users\eduar\source\repos\DistribuidoraSolares"
```

Inicializa el repositorio:

```bash
git init
```

Opcional: define tu nombre y email para los commits (si no lo tienes ya):

```bash
git config user.name "Tu Nombre"
git config user.email "tu@email.com"
```

---

## Paso 3: Primera subida (commit)

Agrega todo lo que no esté en `.gitignore`:

```bash
git add .
```

Revisa qué se va a subir (no deberían aparecer `appsettings.json` ni carpetas `bin/obj`):

```bash
git status
```

Crea el primer commit:

```bash
git commit -m "Proyecto inicial Distribuidora Solares"
```

---

## Paso 4: Crear el repositorio en GitHub

1. Entra en [github.com](https://github.com) e inicia sesión.
2. Clic en **+** (arriba a la derecha) → **New repository**.
3. **Repository name**: por ejemplo `DistribuidoraSolares`.
4. **Description**: opcional, ej. "Sistema de gestión para distribuidora".
5. Elige **Public** (o Private si quieres que solo tú lo veas).
6. **No** marques "Add a README" ni ".gitignore" ni "License" (ya los tienes en tu proyecto).
7. Clic en **Create repository**.

En la página del repo nuevo verás la **URL**, algo como:

- `https://github.com/TU_USUARIO/DistribuidoraSolares.git`

o

- `git@github.com:TU_USUARIO/DistribuidoraSolares.git`

Cópiala para el siguiente paso.

---

## Paso 5: Conectar tu carpeta local con GitHub y subir

En la misma carpeta del proyecto (`DistribuidoraSolares`), ejecuta (sustituye por tu URL y tu rama si usas otra):

```bash
git remote add origin https://github.com/TU_USUARIO/DistribuidoraSolares.git
```

Si tu repositorio se llama distinto o está en una organización, cambia `TU_USUARIO` y el nombre del repo.

Confirma que la rama se llame `main` (o `master` si usas esa):

```bash
git branch -M main
```

Sube el código:

```bash
git push -u origin main
```

La primera vez te pedirá usuario y contraseña de GitHub. En lugar de contraseña suele usarse un **Personal Access Token**:

- GitHub → **Settings** → **Developer settings** → **Personal access tokens** → **Tokens (classic)** → **Generate new token**.
- Marca al menos el permiso **repo**.
- Usa ese token como “contraseña” cuando Git te lo pida.

---

## Resumen de comandos (copiar y pegar)

Sustituye `TU_USUARIO` y `DistribuidoraSolares` por tu usuario y nombre del repo.

```bash
cd "c:\Users\eduar\source\repos\DistribuidoraSolares"
git init
git add .
git status
git commit -m "Proyecto inicial Distribuidora Solares"
git remote add origin https://github.com/TU_USUARIO/DistribuidoraSolares.git
git branch -M main
git push -u origin main
```

El `git status` es opcional pero recomendado para comprobar que no se suben `appsettings.json` ni archivos sensibles.

---

## Después de la primera subida

Para subir cambios más adelante:

```bash
git add .
git commit -m "Descripción breve de lo que cambiaste"
git push
```

---

## Si ya tenías un repo en GitHub y la carpeta ya tiene Git

Si en lugar de "New repository" vacío usaste "Import" o ya habías hecho `git init` y `git remote add` antes:

1. Comprueba el remoto:

   ```bash
   git remote -v
   ```

2. Si `origin` apunta a otro repo, puedes cambiarlo:

   ```bash
   git remote set-url origin https://github.com/TU_USUARIO/DistribuidoraSolares.git
   ```

3. Luego:

   ```bash
   git add .
   git commit -m "Proyecto inicial Distribuidora Solares"
   git push -u origin main
   ```

---

## Si falla "Error al insertar en el repositorio remoto"

Suele ser por **autenticación** o por **rama**. Prueba esto:

### 1. Probar desde PowerShell (para ver el error real)

Abre **PowerShell** y ejecuta:

```powershell
cd "c:\Users\eduar\source\repos\DistribuidoraSolares"
git push -u origin master
```

Si tu repo en GitHub usa la rama **main** y no **master**, prueba:

```powershell
git branch -M main
git push -u origin main
```

Anota el mensaje que salga (por ejemplo: "Authentication failed", "Permission denied", "support for password authentication was removed").

### 2. Usar Personal Access Token (PAT) en lugar de contraseña

GitHub ya no acepta la contraseña de la cuenta para HTTPS. Hay que usar un **token**:

1. En GitHub: **Settings** (tu perfil) → **Developer settings** → **Personal access tokens** → **Tokens (classic)**.
2. **Generate new token (classic)**.
3. Nombre: por ejemplo "DistribuidoraSolares".
4. Marca el permiso **repo**.
5. **Generate token** y **cópialo** (solo se muestra una vez).

Al hacer `git push`, cuando pida:
- **Username**: tu usuario de GitHub (ej. `Edu28933`).
- **Password**: pega el **token**, no la contraseña de la cuenta.

### 3. En Visual Studio

Si sueles usar la ventana "Cambios de Git" de Visual Studio:

1. **Ver** → **Otras ventanas** → **Resultados de Git** (o la ventana donde salga el detalle del error) y revisa el texto completo del fallo.
2. Si dice algo de "authentication" o "credential": en **Configuración de Git** (o en el Administrador de credenciales de Windows) borra las credenciales guardadas de `github.com` y vuelve a hacer "Enviar cambios"; que te pida de nuevo usuario y contraseña y usa el **PAT** como contraseña.
3. Si quieres, haz el push desde PowerShell con los comandos de arriba; suele dar mensajes más claros.

### 4. Comprobar que el repositorio existe

Abre en el navegador:

`https://github.com/Edu28933/DistribuidoraSolares`

Si sale 404, el repositorio no existe o no tienes acceso. Crea el repo en GitHub (sin README ni .gitignore) y vuelve a intentar el push.

---

## Comprobación rápida

- **`.gitignore`** debe incluir `**/appsettings.json` y `**/appsettings.Development.json`.
- **`appsettings.example.json`** sí puede estar en el repo como plantilla.
- No debe haber en GitHub contraseñas ni cadenas de conexión reales.

Si sigues estos pasos, el proyecto quedará en GitHub y podrás usarlo para trabajar en equipo, hacer backup o enlazarlo con Azure u otros servicios.
