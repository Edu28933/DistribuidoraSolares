-- Script para crear tablas de permisos y correos de usuario

-- Tabla para almacenar correos electrónicos de usuarios (separada de tbl_usuarios)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tbl_usuario_emails]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[tbl_usuario_emails](
        [UsuarioEmailId] [int] IDENTITY(1,1) NOT NULL,
        [UsuarioId] [int] NOT NULL,
        [Email] [nvarchar](255) NOT NULL,
        [FechaCreacion] [datetime] NOT NULL DEFAULT GETDATE(),
        [Estado] [varchar](20) NOT NULL DEFAULT 'ACTIVO',
        CONSTRAINT [PK_tbl_usuario_emails] PRIMARY KEY CLUSTERED ([UsuarioEmailId] ASC),
        CONSTRAINT [FK_tbl_usuario_emails_UsuarioId] FOREIGN KEY([UsuarioId]) REFERENCES [dbo].[tbl_usuarios] ([UsuarioId]),
        CONSTRAINT [UQ_tbl_usuario_emails_Email] UNIQUE ([Email])
    );
    
    CREATE INDEX [IX_tbl_usuario_emails_UsuarioId] ON [dbo].[tbl_usuario_emails] ([UsuarioId]);
END
GO

-- Tabla para definir las pantallas disponibles en el sistema
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tbl_pantallas]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[tbl_pantallas](
        [PantallaId] [int] IDENTITY(1,1) NOT NULL,
        [Nombre] [nvarchar](100) NOT NULL,
        [Controlador] [nvarchar](50) NOT NULL,
        [Accion] [nvarchar](50) NULL,
        [Descripcion] [nvarchar](255) NULL,
        [Estado] [varchar](20) NOT NULL DEFAULT 'ACTIVO',
        CONSTRAINT [PK_tbl_pantallas] PRIMARY KEY CLUSTERED ([PantallaId] ASC),
        CONSTRAINT [UQ_tbl_pantallas_Controlador_Accion] UNIQUE ([Controlador], [Accion])
    );
END
GO

-- Tabla para permisos de usuarios por pantalla (SOLO POR ROL)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tbl_permisos_pantalla]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[tbl_permisos_pantalla](
        [PermisoPantallaId] [int] IDENTITY(1,1) NOT NULL,
        [RolId] [int] NOT NULL,
        [PantallaId] [int] NOT NULL,
        [PuedeVer] [bit] NOT NULL DEFAULT 1,
        [PuedeCrear] [bit] NOT NULL DEFAULT 0,
        [PuedeEditar] [bit] NOT NULL DEFAULT 0,
        [PuedeEliminar] [bit] NOT NULL DEFAULT 0,
        [FechaCreacion] [datetime] NOT NULL DEFAULT GETDATE(),
        [Estado] [varchar](20) NOT NULL DEFAULT 'ACTIVO',
        CONSTRAINT [PK_tbl_permisos_pantalla] PRIMARY KEY CLUSTERED ([PermisoPantallaId] ASC),
        CONSTRAINT [FK_tbl_permisos_pantalla_RolId] FOREIGN KEY([RolId]) REFERENCES [dbo].[tbl_roles] ([RolId]),
        CONSTRAINT [FK_tbl_permisos_pantalla_PantallaId] FOREIGN KEY([PantallaId]) REFERENCES [dbo].[tbl_pantallas] ([PantallaId]),
        CONSTRAINT [UQ_tbl_permisos_pantalla_RolId_PantallaId] UNIQUE ([RolId], [PantallaId])
    );
    
    CREATE INDEX [IX_tbl_permisos_pantalla_RolId] ON [dbo].[tbl_permisos_pantalla] ([RolId]);
    CREATE INDEX [IX_tbl_permisos_pantalla_PantallaId] ON [dbo].[tbl_permisos_pantalla] ([PantallaId]);
END
ELSE
BEGIN
    -- Si la tabla ya existe, eliminar columna UsuarioId si existe
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[tbl_permisos_pantalla]') AND name = 'UsuarioId')
    BEGIN
        -- Eliminar constraint si existe
        IF EXISTS (SELECT * FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID(N'[dbo].[tbl_permisos_pantalla]') AND name = 'FK_tbl_permisos_pantalla_UsuarioId')
        BEGIN
            ALTER TABLE [dbo].[tbl_permisos_pantalla] DROP CONSTRAINT [FK_tbl_permisos_pantalla_UsuarioId];
        END
        
        -- Eliminar índice si existe
        IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[tbl_permisos_pantalla]') AND name = 'IX_tbl_permisos_pantalla_UsuarioId')
        BEGIN
            DROP INDEX [IX_tbl_permisos_pantalla_UsuarioId] ON [dbo].[tbl_permisos_pantalla];
        END
        
        -- Eliminar constraint de check si existe
        IF EXISTS (SELECT * FROM sys.check_constraints WHERE parent_object_id = OBJECT_ID(N'[dbo].[tbl_permisos_pantalla]') AND name = 'CK_tbl_permisos_pantalla_UsuarioId_RolId')
        BEGIN
            ALTER TABLE [dbo].[tbl_permisos_pantalla] DROP CONSTRAINT [CK_tbl_permisos_pantalla_UsuarioId_RolId];
        END
        
        -- Eliminar la columna
        ALTER TABLE [dbo].[tbl_permisos_pantalla] DROP COLUMN [UsuarioId];
    END
    
    -- Asegurar que RolId es NOT NULL
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[tbl_permisos_pantalla]') AND name = 'RolId' AND is_nullable = 1)
    BEGIN
        -- Eliminar registros con RolId NULL
        DELETE FROM [dbo].[tbl_permisos_pantalla] WHERE RolId IS NULL;
        
        -- Eliminar índice si existe antes de modificar la columna
        IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[tbl_permisos_pantalla]') AND name = 'IX_tbl_permisos_pantalla_RolId')
        BEGIN
            DROP INDEX [IX_tbl_permisos_pantalla_RolId] ON [dbo].[tbl_permisos_pantalla];
        END
        
        -- Cambiar a NOT NULL
        ALTER TABLE [dbo].[tbl_permisos_pantalla] ALTER COLUMN [RolId] [int] NOT NULL;
        
        -- Recrear el índice después de modificar la columna
        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[tbl_permisos_pantalla]') AND name = 'IX_tbl_permisos_pantalla_RolId')
        BEGIN
            CREATE INDEX [IX_tbl_permisos_pantalla_RolId] ON [dbo].[tbl_permisos_pantalla] ([RolId]);
        END
    END
    
    -- Agregar constraint único si no existe
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[tbl_permisos_pantalla]') AND name = 'UQ_tbl_permisos_pantalla_RolId_PantallaId')
    BEGIN
        ALTER TABLE [dbo].[tbl_permisos_pantalla] ADD CONSTRAINT [UQ_tbl_permisos_pantalla_RolId_PantallaId] UNIQUE ([RolId], [PantallaId]);
    END
END
GO

-- Insertar pantallas del sistema
IF NOT EXISTS (SELECT 1 FROM tbl_pantallas)
BEGIN
    INSERT INTO tbl_pantallas (Nombre, Controlador, Accion, Descripcion) VALUES
    ('Dashboard', 'Dashboard', 'Index', 'Panel principal'),
    ('Categorías', 'Categorias', 'Index', 'Gestión de categorías'),
    ('Productos', 'Productos', 'Index', 'Gestión de productos'),
    ('Compras', 'Compras', 'Index', 'Gestión de compras'),
    ('Ventas', 'Ventas', 'Index', 'Gestión de ventas'),
    ('Reservas', 'Reservas', 'Index', 'Gestión de reservas'),
    ('Caja', 'Caja', 'Index', 'Gestión de caja'),
    ('Clientes', 'Clientes', 'Index', 'Gestión de clientes'),
    ('Proveedores', 'Proveedores', 'Index', 'Gestión de proveedores'),
    ('Usuarios', 'Usuarios', 'Index', 'Gestión de usuarios'),
    ('Roles', 'Roles', 'Index', 'Gestión de roles'),
    ('Métodos de Pago', 'MetodosPago', 'Index', 'Gestión de métodos de pago'),
    ('Movimientos de Inventario', 'MovimientosInventario', 'Index', 'Consulta de movimientos de inventario'),
    ('Permisos', 'Permisos', 'Index', 'Gestión de permisos por pantalla');
END
GO

-- Stored Procedures para gestión de correos de usuario
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_usuario_emails_obtener]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[usp_usuario_emails_obtener]
GO

CREATE PROCEDURE [dbo].[usp_usuario_emails_obtener]
    @UsuarioId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT UsuarioEmailId, UsuarioId, Email, FechaCreacion, Estado
    FROM tbl_usuario_emails
    WHERE UsuarioId = @UsuarioId AND Estado = 'ACTIVO';
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_usuario_emails_crear]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[usp_usuario_emails_crear]
GO

CREATE PROCEDURE [dbo].[usp_usuario_emails_crear]
    @UsuarioId INT,
    @Email NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Verificar si el email ya existe
    IF EXISTS (SELECT 1 FROM tbl_usuario_emails WHERE Email = @Email AND Estado = 'ACTIVO')
    BEGIN
        RAISERROR('El correo electrónico ya está registrado', 16, 1);
        RETURN;
    END
    
    -- Desactivar emails anteriores del usuario
    UPDATE tbl_usuario_emails SET Estado = 'INACTIVO' WHERE UsuarioId = @UsuarioId AND Estado = 'ACTIVO';
    
    -- Crear nuevo email
    INSERT INTO tbl_usuario_emails (UsuarioId, Email)
    VALUES (@UsuarioId, @Email);
    
    SELECT SCOPE_IDENTITY() AS UsuarioEmailId;
END
GO

-- Stored Procedures para gestión de permisos (SOLO POR ROL)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_permisos_pantalla_obtener_por_rol]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[usp_permisos_pantalla_obtener_por_rol]
GO

CREATE PROCEDURE [dbo].[usp_permisos_pantalla_obtener_por_rol]
    @RolId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Obtener permisos del rol
    SELECT 
        pp.PermisoPantallaId,
        pp.RolId,
        pp.PantallaId,
        pp.PuedeVer,
        pp.PuedeCrear,
        pp.PuedeEditar,
        pp.PuedeEliminar,
        pp.FechaCreacion,
        pp.Estado
    FROM tbl_permisos_pantalla pp
    WHERE pp.RolId = @RolId
      AND pp.Estado = 'ACTIVO';
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_permisos_pantalla_obtener_por_usuario]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[usp_permisos_pantalla_obtener_por_usuario]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_permisos_pantalla_guardar]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[usp_permisos_pantalla_guardar]
GO

CREATE PROCEDURE [dbo].[usp_permisos_pantalla_guardar]
    @RolId INT,
    @PantallaId INT,
    @PuedeVer BIT,
    @PuedeCrear BIT,
    @PuedeEditar BIT,
    @PuedeEliminar BIT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Buscar si ya existe un permiso
    DECLARE @PermisoPantallaId INT;
    SELECT @PermisoPantallaId = PermisoPantallaId
    FROM tbl_permisos_pantalla
    WHERE PantallaId = @PantallaId
      AND RolId = @RolId
      AND Estado = 'ACTIVO';
    
    IF @PermisoPantallaId IS NOT NULL
    BEGIN
        -- Actualizar
        UPDATE tbl_permisos_pantalla
        SET PuedeVer = @PuedeVer,
            PuedeCrear = @PuedeCrear,
            PuedeEditar = @PuedeEditar,
            PuedeEliminar = @PuedeEliminar
        WHERE PermisoPantallaId = @PermisoPantallaId;
    END
    ELSE
    BEGIN
        -- Crear nuevo
        INSERT INTO tbl_permisos_pantalla (RolId, PantallaId, PuedeVer, PuedeCrear, PuedeEditar, PuedeEliminar)
        VALUES (@RolId, @PantallaId, @PuedeVer, @PuedeCrear, @PuedeEditar, @PuedeEliminar);
        
        SET @PermisoPantallaId = SCOPE_IDENTITY();
    END
    
    SELECT @PermisoPantallaId AS PermisoPantallaId;
END
GO

-- Stored Procedure para recuperación de contraseña
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_usuario_recuperar_password]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[usp_usuario_recuperar_password]
GO

CREATE PROCEDURE [dbo].[usp_usuario_recuperar_password]
    @Email NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @UsuarioId INT;
    
    -- Buscar usuario por email
    SELECT @UsuarioId = ue.UsuarioId
    FROM tbl_usuario_emails ue
    INNER JOIN tbl_usuarios u ON u.UsuarioId = ue.UsuarioId
    WHERE ue.Email = @Email 
      AND ue.Estado = 'ACTIVO'
      AND u.Estado = 'ACTIVO';
    
    IF @UsuarioId IS NULL
    BEGIN
        RAISERROR('No se encontró un usuario activo con ese correo electrónico', 16, 1);
        RETURN;
    END
    
    -- Generar nueva contraseña temporal (8 caracteres alfanuméricos)
    DECLARE @NuevaPassword NVARCHAR(8);
    DECLARE @RandomValue UNIQUEIDENTIFIER = NEWID();
    -- Convertir GUID a string y tomar primeros 8 caracteres alfanuméricos
    SET @NuevaPassword = UPPER(SUBSTRING(REPLACE(CONVERT(NVARCHAR(36), @RandomValue), '-', ''), 1, 8));
    
    -- Retornar la contraseña en texto plano (el hash se calculará en C#)
    SELECT @NuevaPassword AS NuevaPassword, @UsuarioId AS UsuarioId;
END
GO
