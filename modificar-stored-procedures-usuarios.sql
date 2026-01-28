-- =============================================
-- MODIFICAR STORED PROCEDURES DE USUARIOS
-- Agregar parámetro @SucursalId
-- =============================================

USE [bd_distribuidora_solares];
GO

-- =============================================
-- Modificar usp_usuarios_crear
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_usuarios_crear]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[usp_usuarios_crear];
    PRINT 'Stored procedure usp_usuarios_crear eliminado para modificación';
END
GO

CREATE PROCEDURE [dbo].[usp_usuarios_crear]
    @RolId INT,
    @Nombre VARCHAR(150),
    @UsuarioLogin VARCHAR(50),
    @PasswordHash VARCHAR(MAX),
    @Estado VARCHAR(15) = 'ACTIVO',
    @SucursalId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validar que el usuario login no exista
    IF EXISTS (SELECT 1 FROM tbl_usuarios WHERE UsuarioLogin = @UsuarioLogin)
    BEGIN
        THROW 50001, 'El nombre de usuario ya existe', 1;
    END
    
    INSERT INTO dbo.tbl_usuarios (RolId, Nombre, UsuarioLogin, PasswordHash, Estado, SucursalId)
    VALUES (@RolId, @Nombre, @UsuarioLogin, @PasswordHash, @Estado, @SucursalId);
    
    SELECT SCOPE_IDENTITY() AS UsuarioId;
END
GO

PRINT 'Stored procedure usp_usuarios_crear modificado exitosamente';
GO

-- =============================================
-- Modificar usp_usuarios_editar
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_usuarios_editar]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[usp_usuarios_editar];
    PRINT 'Stored procedure usp_usuarios_editar eliminado para modificación';
END
GO

CREATE PROCEDURE [dbo].[usp_usuarios_editar]
    @UsuarioId INT,
    @RolId INT,
    @Nombre VARCHAR(150),
    @UsuarioLogin VARCHAR(50),
    @PasswordHash VARCHAR(MAX) = NULL,
    @Estado VARCHAR(15),
    @SucursalId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validar que el usuario login no exista en otro usuario
    IF EXISTS (SELECT 1 FROM tbl_usuarios WHERE UsuarioLogin = @UsuarioLogin AND UsuarioId <> @UsuarioId)
    BEGIN
        THROW 50002, 'El nombre de usuario ya existe en otro usuario', 1;
    END
    
    -- Si PasswordHash es NULL, mantener el actual
    IF @PasswordHash IS NULL
    BEGIN
        UPDATE dbo.tbl_usuarios
        SET RolId = @RolId,
            Nombre = @Nombre,
            UsuarioLogin = @UsuarioLogin,
            Estado = @Estado,
            SucursalId = @SucursalId
        WHERE UsuarioId = @UsuarioId;
    END
    ELSE
    BEGIN
        UPDATE dbo.tbl_usuarios
        SET RolId = @RolId,
            Nombre = @Nombre,
            UsuarioLogin = @UsuarioLogin,
            PasswordHash = @PasswordHash,
            Estado = @Estado,
            SucursalId = @SucursalId
        WHERE UsuarioId = @UsuarioId;
    END
    
    SELECT @@ROWCOUNT;
END
GO

PRINT 'Stored procedure usp_usuarios_editar modificado exitosamente';
GO

PRINT '========================================';
PRINT 'STORED PROCEDURES DE USUARIOS MODIFICADOS';
PRINT '========================================';
GO
