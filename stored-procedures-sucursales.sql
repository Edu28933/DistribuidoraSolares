-- =============================================
-- STORED PROCEDURES PARA SUCURSALES
-- Sistema Distribuidora Solares - Multi-Sucursal
-- =============================================

USE [bd_distribuidora_solares];
GO

-- =============================================
-- usp_sucursales_mostrar
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_sucursales_mostrar]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[usp_sucursales_mostrar];
GO

CREATE PROCEDURE [dbo].[usp_sucursales_mostrar]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT *
    FROM dbo.tbl_sucursales
    ORDER BY Nombre;
END
GO

-- =============================================
-- usp_sucursales_crear
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_sucursales_crear]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[usp_sucursales_crear];
GO

CREATE PROCEDURE [dbo].[usp_sucursales_crear]
    @Nombre VARCHAR(150),
    @Codigo VARCHAR(20) = NULL,
    @Direccion VARCHAR(255) = NULL,
    @Telefono VARCHAR(30) = NULL,
    @Estado VARCHAR(15) = 'ACTIVO'
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validar que el código no exista si se proporciona
    IF @Codigo IS NOT NULL AND EXISTS (SELECT 1 FROM tbl_sucursales WHERE Codigo = @Codigo)
    BEGIN
        THROW 60001, 'El código de sucursal ya existe.', 1;
    END
    
    INSERT INTO dbo.tbl_sucursales (Nombre, Codigo, Direccion, Telefono, Estado)
    VALUES (@Nombre, @Codigo, @Direccion, @Telefono, @Estado);
    
    SELECT SCOPE_IDENTITY() AS SucursalId;
END
GO

-- =============================================
-- usp_sucursales_editar
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_sucursales_editar]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[usp_sucursales_editar];
GO

CREATE PROCEDURE [dbo].[usp_sucursales_editar]
    @SucursalId INT,
    @Nombre VARCHAR(150),
    @Codigo VARCHAR(20) = NULL,
    @Direccion VARCHAR(255) = NULL,
    @Telefono VARCHAR(30) = NULL,
    @Estado VARCHAR(15)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validar que el código no exista en otra sucursal si se proporciona
    IF @Codigo IS NOT NULL AND EXISTS (
        SELECT 1 FROM tbl_sucursales 
        WHERE Codigo = @Codigo AND SucursalId <> @SucursalId
    )
    BEGIN
        THROW 60002, 'El código de sucursal ya existe en otra sucursal.', 1;
    END
    
    UPDATE dbo.tbl_sucursales
    SET Nombre = @Nombre,
        Codigo = @Codigo,
        Direccion = @Direccion,
        Telefono = @Telefono,
        Estado = @Estado
    WHERE SucursalId = @SucursalId;
    
    SELECT @@ROWCOUNT;
END
GO

-- =============================================
-- usp_sucursales_eliminar (eliminación lógica)
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_sucursales_eliminar]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[usp_sucursales_eliminar];
GO

CREATE PROCEDURE [dbo].[usp_sucursales_eliminar]
    @SucursalId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Verificar que no haya usuarios asignados a esta sucursal
    IF EXISTS (SELECT 1 FROM tbl_usuarios WHERE SucursalId = @SucursalId AND Estado = 'ACTIVO')
    BEGIN
        THROW 60003, 'No se puede eliminar la sucursal: tiene usuarios activos asignados.', 1;
    END
    
    UPDATE dbo.tbl_sucursales
    SET Estado = 'INACTIVO'
    WHERE SucursalId = @SucursalId;
    
    SELECT @@ROWCOUNT;
END
GO

-- =============================================
-- usp_sucursales_buscar
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_sucursales_buscar]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[usp_sucursales_buscar];
GO

CREATE PROCEDURE [dbo].[usp_sucursales_buscar]
    @Buscar VARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT *
    FROM dbo.tbl_sucursales
    WHERE CAST(SucursalId AS VARCHAR(20)) LIKE '%' + @Buscar + '%'
       OR Nombre LIKE '%' + @Buscar + '%'
       OR ISNULL(Codigo, '') LIKE '%' + @Buscar + '%'
       OR ISNULL(Direccion, '') LIKE '%' + @Buscar + '%'
       OR Estado LIKE '%' + @Buscar + '%'
    ORDER BY Nombre;
END
GO

-- =============================================
-- usp_sucursales_obtener_por_id
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_sucursales_obtener_por_id]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[usp_sucursales_obtener_por_id];
GO

CREATE PROCEDURE [dbo].[usp_sucursales_obtener_por_id]
    @SucursalId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT *
    FROM dbo.tbl_sucursales
    WHERE SucursalId = @SucursalId;
END
GO

PRINT 'Stored procedures de sucursales creados exitosamente';
GO
