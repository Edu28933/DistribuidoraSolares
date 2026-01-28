-- =============================================
-- MODIFICAR STORED PROCEDURES DE ANULACIÓN/CANCELACIÓN
-- Agregar SucursalId a movimientos de inventario
-- =============================================

USE [bd_distribuidora_solares];
GO

-- =============================================
-- Modificar usp_venta_anular
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_venta_anular]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[usp_venta_anular];
    PRINT 'Stored procedure usp_venta_anular eliminado para modificación';
END
GO

CREATE PROCEDURE [dbo].[usp_venta_anular]
    @VentaId INT,
    @UsuarioId INT,
    @Motivo VARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRAN;

        IF NOT EXISTS (SELECT 1 FROM dbo.tbl_usuarios WHERE UsuarioId=@UsuarioId AND Estado='ACTIVO')
            THROW 52003, 'Usuario no válido o inactivo.', 1;

        /* Obtener SucursalId de la venta */
        DECLARE @SucursalIdVenta INT;
        SELECT @SucursalIdVenta = SucursalId
        FROM dbo.tbl_ventas
        WHERE VentaId = @VentaId;

        /* Tomar lock y asegurar que está ACTIVO */
        UPDATE dbo.tbl_ventas
        SET Observacion = Observacion
        WHERE VentaId=@VentaId AND Estado='ACTIVO';

        IF @@ROWCOUNT = 0
            THROW 52001, 'La venta no existe o ya está anulada.', 1;

        /* Revertir inventario: AJUSTE positivo */
        INSERT INTO dbo.tbl_inventario_movimientos
        (ProductoId, FechaHora, Tipo, Cantidad, CostoUnitario, VentaId, CompraId, ReservaId, Descripcion, UsuarioId, Estado, SucursalId)
        SELECT
            vd.ProductoId,
            GETDATE(),
            'AJUSTE',
            vd.Cantidad,
            NULL,
            @VentaId,
            NULL,
            NULL,
            CONCAT('Reverso por anulación de venta. ', ISNULL(@Motivo,'')),
            @UsuarioId,
            'ACTIVO',
            @SucursalIdVenta
        FROM dbo.tbl_venta_detalle vd
        WHERE vd.VentaId = @VentaId;

        /* Anular movimiento de caja ligado a la venta */
        UPDATE dbo.tbl_caja_movimientos
        SET Estado = 'ANULADO'
        WHERE VentaId = @VentaId
          AND Estado = 'ACTIVO';

        /* Anular venta */
        UPDATE dbo.tbl_ventas
        SET Estado = 'ANULADO',
            Observacion = CONCAT(ISNULL(Observacion,''), ' | ANULADA: ', ISNULL(@Motivo,''))
        WHERE VentaId = @VentaId;

        COMMIT;
        SELECT VentaId = @VentaId, Estado = 'ANULADO';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        THROW;
    END CATCH
END
GO

PRINT 'Stored procedure usp_venta_anular modificado exitosamente';
GO

-- =============================================
-- Modificar usp_compra_anular
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_compra_anular]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[usp_compra_anular];
    PRINT 'Stored procedure usp_compra_anular eliminado para modificación';
END
GO

CREATE PROCEDURE [dbo].[usp_compra_anular]
    @CompraId INT,
    @UsuarioId INT,
    @Motivo VARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRAN;

        IF NOT EXISTS (SELECT 1 FROM dbo.tbl_usuarios WHERE UsuarioId=@UsuarioId AND Estado='ACTIVO')
            THROW 54003, 'Usuario no válido o inactivo.', 1;

        /* Obtener SucursalId de la compra */
        DECLARE @SucursalIdCompra INT;
        SELECT @SucursalIdCompra = SucursalId
        FROM dbo.tbl_compras
        WHERE CompraId = @CompraId;

        /* Tomar lock y asegurar que está ACTIVO */
        UPDATE dbo.tbl_compras
        SET Total = Total
        WHERE CompraId=@CompraId AND Estado='ACTIVO';

        IF @@ROWCOUNT = 0
            THROW 54001, 'La compra no existe o ya está anulada.', 1;

        /* Validar que anular no deje stock negativo */
        IF EXISTS (
            SELECT 1
            FROM dbo.tbl_compra_detalle cd
            WHERE cd.CompraId = @CompraId
            AND EXISTS (
                SELECT 1
                FROM (
                    SELECT StockActual = ISNULL(SUM(im.Cantidad),0)
                    FROM dbo.tbl_inventario_movimientos im WITH (UPDLOCK, HOLDLOCK)
                    WHERE im.ProductoId = cd.ProductoId
                      AND im.Estado = 'ACTIVO'
                      AND (@SucursalIdCompra IS NULL OR im.SucursalId = @SucursalIdCompra)
                ) s
                WHERE s.StockActual < cd.Cantidad
            )
        )
            THROW 54002, 'No se puede anular la compra: parte del stock ya fue consumido (quedaría negativo).', 1;

        /* Revertir inventario: AJUSTE negativo */
        INSERT INTO dbo.tbl_inventario_movimientos
        (ProductoId, FechaHora, Tipo, Cantidad, CostoUnitario, VentaId, CompraId, ReservaId, Descripcion, UsuarioId, Estado, SucursalId)
        SELECT
            cd.ProductoId,
            GETDATE(),
            'AJUSTE',
            -cd.Cantidad,
            cd.CostoUnitario,
            NULL,
            @CompraId,
            NULL,
            CONCAT('Reverso por anulación de compra. ', ISNULL(@Motivo,'')),
            @UsuarioId,
            'ACTIVO',
            @SucursalIdCompra
        FROM dbo.tbl_compra_detalle cd
        WHERE cd.CompraId = @CompraId;

        /* Anular movimiento de caja */
        UPDATE dbo.tbl_caja_movimientos
        SET Estado = 'ANULADO'
        WHERE CompraId = @CompraId
          AND Estado = 'ACTIVO';

        /* Anular compra */
        UPDATE dbo.tbl_compras
        SET Estado = 'ANULADO'
        WHERE CompraId = @CompraId;

        COMMIT;
        SELECT CompraId = @CompraId, Estado = 'ANULADO';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        THROW;
    END CATCH
END
GO

PRINT 'Stored procedure usp_compra_anular modificado exitosamente';
GO

-- =============================================
-- Modificar usp_reserva_cancelar
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_reserva_cancelar]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[usp_reserva_cancelar];
    PRINT 'Stored procedure usp_reserva_cancelar eliminado para modificación';
END
GO

CREATE PROCEDURE [dbo].[usp_reserva_cancelar]
    @ReservaId INT,
    @UsuarioId INT,
    @Motivo    VARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRAN;

        IF NOT EXISTS (SELECT 1 FROM dbo.tbl_usuarios WHERE UsuarioId=@UsuarioId AND Estado='ACTIVO')
            THROW 56002, 'Usuario no válido o inactivo.', 1;

        /* Obtener SucursalId de la reserva */
        DECLARE @SucursalIdReserva INT;
        SELECT @SucursalIdReserva = SucursalId
        FROM dbo.tbl_reservas
        WHERE ReservaId = @ReservaId;

        IF NOT EXISTS (SELECT 1 FROM dbo.tbl_reservas WHERE ReservaId=@ReservaId AND Estado='ACTIVA')
            THROW 56001, 'La reserva no existe o no está ACTIVA.', 1;

        INSERT INTO dbo.tbl_inventario_movimientos
        (ProductoId, FechaHora, Tipo, Cantidad, CostoUnitario, VentaId, CompraId, ReservaId, Descripcion, UsuarioId, Estado, SucursalId)
        SELECT
            rd.ProductoId,
            GETDATE(),
            'LIBERACION_RESERVA',
            rd.CantidadReservada,
            NULL,
            NULL,
            NULL,
            @ReservaId,
            CONCAT('Liberación por CANCELACIÓN. ', ISNULL(@Motivo,'')),
            @UsuarioId,
            'ACTIVO',
            @SucursalIdReserva
        FROM dbo.tbl_reserva_detalle rd
        WHERE rd.ReservaId=@ReservaId AND rd.Estado='ACTIVO';

        UPDATE dbo.tbl_reserva_detalle
        SET Estado='CANCELADO'
        WHERE ReservaId=@ReservaId AND Estado='ACTIVO';

        UPDATE dbo.tbl_reservas
        SET Estado='CANCELADA',
            Observacion = CONCAT(ISNULL(Observacion,''), ' | CANCELADA: ', ISNULL(@Motivo,''))
        WHERE ReservaId=@ReservaId;

        COMMIT;

        SELECT ReservaId=@ReservaId, Estado='CANCELADA';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        THROW;
    END CATCH
END
GO

PRINT 'Stored procedure usp_reserva_cancelar modificado exitosamente';
GO

-- =============================================
-- Modificar usp_inventario_movimientos_crear
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_inventario_movimientos_crear]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[usp_inventario_movimientos_crear];
    PRINT 'Stored procedure usp_inventario_movimientos_crear eliminado para modificación';
END
GO

CREATE PROCEDURE [dbo].[usp_inventario_movimientos_crear]
    @ProductoId INT,
    @Tipo VARCHAR(30),
    @Cantidad INT,
    @CostoUnitario DECIMAL(10,2) = NULL,
    @VentaId INT = NULL,
    @CompraId INT = NULL,
    @ReservaId INT = NULL,
    @Descripcion VARCHAR(255) = NULL,
    @UsuarioId INT,
    @Estado VARCHAR(15),
    @SucursalId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    /* Calcular SucursalId */
    DECLARE @SucursalIdCalculado INT;
    IF @SucursalId IS NULL
    BEGIN
        SELECT @SucursalIdCalculado = SucursalId 
        FROM dbo.tbl_usuarios 
        WHERE UsuarioId = @UsuarioId;
    END
    ELSE
    BEGIN
        SET @SucursalIdCalculado = @SucursalId;
    END

    INSERT INTO dbo.tbl_inventario_movimientos
    (ProductoId, Tipo, Cantidad, CostoUnitario, VentaId, CompraId, ReservaId, Descripcion, UsuarioId, Estado, SucursalId)
    VALUES
    (@ProductoId, @Tipo, @Cantidad, @CostoUnitario, @VentaId, @CompraId, @ReservaId, @Descripcion, @UsuarioId, @Estado, @SucursalIdCalculado);

    SELECT SCOPE_IDENTITY() AS InvMovId;
END
GO

PRINT 'Stored procedure usp_inventario_movimientos_crear modificado exitosamente';
GO

-- =============================================
-- Modificar usp_compra_detalle_crear
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_compra_detalle_crear]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[usp_compra_detalle_crear];
    PRINT 'Stored procedure usp_compra_detalle_crear eliminado para modificación';
END
GO

CREATE PROCEDURE [dbo].[usp_compra_detalle_crear]
    @CompraId INT,
    @ProductoId INT,
    @Cantidad INT,
    @CostoUnitario DECIMAL(10,2),
    @UsuarioId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Subtotal DECIMAL(12,2);
    SET @Subtotal = @Cantidad * @CostoUnitario;

    /* Obtener SucursalId de la compra */
    DECLARE @SucursalIdCompra INT;
    SELECT @SucursalIdCompra = SucursalId
    FROM dbo.tbl_compras
    WHERE CompraId = @CompraId;

    INSERT INTO dbo.tbl_compra_detalle
    (CompraId, ProductoId, Cantidad, CostoUnitario, Subtotal)
    VALUES
    (@CompraId, @ProductoId, @Cantidad, @CostoUnitario, @Subtotal);

    -- Kardex / Inventario
    INSERT INTO dbo.tbl_inventario_movimientos
    (ProductoId, Tipo, Cantidad, CostoUnitario, CompraId, UsuarioId, Estado, SucursalId)
    VALUES
    (@ProductoId, 'COMPRA', @Cantidad, @CostoUnitario, @CompraId, @UsuarioId, 'ACTIVO', @SucursalIdCompra);

    SELECT SCOPE_IDENTITY() AS CompraDetalleId;
END
GO

PRINT 'Stored procedure usp_compra_detalle_crear modificado exitosamente';
GO

PRINT '========================================';
PRINT 'STORED PROCEDURES DE ANULACIÓN MODIFICADOS';
PRINT '========================================';
PRINT 'usp_venta_anular: ✓ Modificado';
PRINT 'usp_compra_anular: ✓ Modificado';
PRINT 'usp_reserva_cancelar: ✓ Modificado';
PRINT 'usp_inventario_movimientos_crear: ✓ Modificado';
PRINT 'usp_compra_detalle_crear: ✓ Modificado';
PRINT '========================================';
GO
