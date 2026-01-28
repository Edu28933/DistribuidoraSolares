-- =============================================
-- MODIFICAR STORED PROCEDURES DE CREACIÓN COMPLETOS
-- Agregar @SucursalId y usarlo en INSERTs
-- =============================================

USE [bd_distribuidora_solares];
GO

-- =============================================
-- Modificar usp_venta_completa
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_venta_completa]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[usp_venta_completa];
    PRINT 'Stored procedure usp_venta_completa eliminado para modificación';
END
GO

CREATE PROCEDURE [dbo].[usp_venta_completa]
    @ClienteId INT = NULL,
    @UsuarioId INT,
    @MetodoPagoId INT,
    @TipoVenta VARCHAR(15),
    @DescuentoManual DECIMAL(12,2) = 0,
    @Observacion VARCHAR(255) = NULL,
    @SucursalId INT = NULL,
    @Detalle dbo.TVP_VentaDetalle READONLY
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRAN;

        /* ===== Validaciones base ===== */
        IF NOT EXISTS (SELECT 1 FROM dbo.tbl_usuarios WHERE UsuarioId=@UsuarioId AND Estado='ACTIVO')
            THROW 51001, 'Usuario no válido o inactivo.', 1;

        IF NOT EXISTS (SELECT 1 FROM dbo.tbl_metodos_pago WHERE MetodoPagoId=@MetodoPagoId AND Estado='ACTIVO')
            THROW 51002, 'Método de pago no válido o inactivo.', 1;

        IF @TipoVenta NOT IN ('NORMAL','PERDIDA')
            THROW 51003, 'TipoVenta inválido.', 1;

        IF NOT EXISTS (SELECT 1 FROM @Detalle)
            THROW 51004, 'La venta debe tener al menos 1 item.', 1;

        IF @ClienteId IS NOT NULL
           AND NOT EXISTS (SELECT 1 FROM dbo.tbl_clientes WHERE ClienteId=@ClienteId AND Estado='ACTIVO')
            THROW 51010, 'Cliente no válido o inactivo.', 1;

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

        /* Caja abierta */
        DECLARE @CajaId INT;
        SELECT TOP (1) @CajaId = CajaId
        FROM dbo.tbl_caja WITH (UPDLOCK, HOLDLOCK)
        WHERE Estado = 'ABIERTA'
        ORDER BY CajaId DESC;

        IF @CajaId IS NULL
            THROW 51005, 'No hay caja ABIERTA. Abra caja antes de vender.', 1;

        /* Consolidar detalle por producto */
        DECLARE @Det TABLE(
            ProductoId INT PRIMARY KEY,
            Cantidad INT NOT NULL,
            PrecioLista DECIMAL(10,2) NOT NULL,
            PrecioUnitario DECIMAL(10,2) NOT NULL
        );

        INSERT INTO @Det (ProductoId, Cantidad, PrecioLista, PrecioUnitario)
        SELECT
            d.ProductoId,
            SUM(d.Cantidad) AS Cantidad,
            MAX(d.PrecioLista) AS PrecioLista,
            MAX(d.PrecioUnitario) AS PrecioUnitario
        FROM @Detalle d
        GROUP BY d.ProductoId;

        /* Productos DISPONIBLE */
        IF EXISTS (
            SELECT 1
            FROM @Det d
            JOIN dbo.tbl_productos p ON p.ProductoId = d.ProductoId
            WHERE p.Estado <> 'DISPONIBLE'
        )
            THROW 51006, 'Hay productos NO_DISPONIBLE en el detalle.', 1;

        /* Validar stock suficiente con BLOQUEO para evitar sobreventa */
        IF EXISTS (
            SELECT 1
            FROM @Det d
            OUTER APPLY (
                SELECT StockActual = ISNULL(SUM(im.Cantidad), 0)
                FROM dbo.tbl_inventario_movimientos im WITH (UPDLOCK, HOLDLOCK)
                WHERE im.ProductoId = d.ProductoId
                  AND im.Estado = 'ACTIVO'
                  AND (@SucursalIdCalculado IS NULL OR im.SucursalId = @SucursalIdCalculado)
            ) s
            WHERE s.StockActual < d.Cantidad
        )
            THROW 51007, 'Stock insuficiente para uno o más productos.', 1;

        /* Totales */
        DECLARE @TotalBruto DECIMAL(12,2) =
            (SELECT SUM(CAST(Cantidad AS DECIMAL(12,2)) * PrecioUnitario) FROM @Det);

        IF @TotalBruto IS NULL OR @TotalBruto <= 0
            THROW 51008, 'TotalBruto inválido.', 1;

        IF @DescuentoManual < 0 OR @DescuentoManual > @TotalBruto
            THROW 51009, 'DescuentoManual inválido.', 1;

        DECLARE @TotalNeto DECIMAL(12,2) = @TotalBruto - @DescuentoManual;

        /* ===== Insert Venta ===== */
        INSERT INTO dbo.tbl_ventas
        (FechaHora, ClienteId, UsuarioId, MetodoPagoId, TipoVenta, TotalBruto, DescuentoManual, TotalNeto, Observacion, Estado, SucursalId)
        VALUES
        (GETDATE(), @ClienteId, @UsuarioId, @MetodoPagoId, @TipoVenta, @TotalBruto, @DescuentoManual, @TotalNeto, @Observacion, 'ACTIVO', @SucursalIdCalculado);

        DECLARE @VentaId INT = SCOPE_IDENTITY();

        /* ===== Insert Detalle ===== */
        INSERT INTO dbo.tbl_venta_detalle
        (VentaId, ProductoId, Cantidad, PrecioLista, PrecioUnitario, Subtotal)
        SELECT
            @VentaId,
            d.ProductoId,
            d.Cantidad,
            d.PrecioLista,
            d.PrecioUnitario,
            CAST(d.Cantidad AS DECIMAL(12,2)) * d.PrecioUnitario
        FROM @Det d;

        /* ===== Inventario: salida (negativa) ===== */
        INSERT INTO dbo.tbl_inventario_movimientos
        (ProductoId, FechaHora, Tipo, Cantidad, CostoUnitario, VentaId, CompraId, ReservaId, Descripcion, UsuarioId, Estado, SucursalId)
        SELECT
            d.ProductoId,
            GETDATE(),
            CASE WHEN @TipoVenta='PERDIDA' THEN 'PERDIDA' ELSE 'VENTA' END,
            -d.Cantidad,
            NULL,
            @VentaId,
            NULL,
            NULL,
            CONCAT('Salida por ', CASE WHEN @TipoVenta='PERDIDA' THEN 'PERDIDA' ELSE 'VENTA' END),
            @UsuarioId,
            'ACTIVO',
            @SucursalIdCalculado
        FROM @Det d;

        /* ===== Caja: ingreso ===== */
        INSERT INTO dbo.tbl_caja_movimientos
        (CajaId, FechaHora, Tipo, Concepto, Monto, VentaId, CompraId, UsuarioId, Estado)
        VALUES
        (@CajaId, GETDATE(), 'INGRESO',
         CONCAT('Venta #', @VentaId, ' (', @TipoVenta, ')'),
         @TotalNeto, @VentaId, NULL, @UsuarioId, 'ACTIVO');

        COMMIT;

        SELECT VentaId = @VentaId, TotalNeto = @TotalNeto;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        THROW;
    END CATCH
END
GO

PRINT 'Stored procedure usp_venta_completa modificado exitosamente';
GO

-- =============================================
-- Modificar usp_compra_completa
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_compra_completa]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[usp_compra_completa];
    PRINT 'Stored procedure usp_compra_completa eliminado para modificación';
END
GO

CREATE PROCEDURE [dbo].[usp_compra_completa]
    @ProveedorId INT,
    @UsuarioId   INT,
    @Descripcion VARCHAR(255) = NULL,
    @SucursalId INT = NULL,
    @Detalle     dbo.TVP_CompraDetalle READONLY
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRAN;

        /* ===== Validaciones ===== */
        IF NOT EXISTS (SELECT 1 FROM dbo.tbl_proveedores WHERE ProveedorId = @ProveedorId AND Estado = 'ACTIVO')
            THROW 53001, 'Proveedor no válido o inactivo.', 1;

        IF NOT EXISTS (SELECT 1 FROM dbo.tbl_usuarios WHERE UsuarioId = @UsuarioId AND Estado = 'ACTIVO')
            THROW 53002, 'Usuario no válido o inactivo.', 1;

        IF NOT EXISTS (SELECT 1 FROM @Detalle)
            THROW 53003, 'La compra debe tener al menos 1 item.', 1;

        /* Validar productos DISPONIBLE */
        IF EXISTS (
            SELECT 1
            FROM @Detalle d
            JOIN dbo.tbl_productos p ON p.ProductoId = d.ProductoId
            WHERE p.Estado <> 'DISPONIBLE'
        )
            THROW 53004, 'Hay productos NO_DISPONIBLE en el detalle.', 1;

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

        /* Caja ABIERTA */
        DECLARE @CajaId INT;
        SELECT TOP (1) @CajaId = CajaId
        FROM dbo.tbl_caja
        WHERE Estado = 'ABIERTA'
        ORDER BY CajaId DESC;

        IF @CajaId IS NULL
            THROW 53005, 'No hay caja ABIERTA para registrar compra.', 1;

        /* Total */
        DECLARE @Total DECIMAL(12,2) =
            (SELECT SUM(CAST(Cantidad AS DECIMAL(12,2)) * CostoUnitario) FROM @Detalle);

        IF @Total IS NULL OR @Total <= 0
            THROW 53006, 'Total de compra inválido.', 1;

        /* ===== Insert Compra ===== */
        INSERT INTO dbo.tbl_compras (ProveedorId, Fecha, Total, UsuarioId, Estado, SucursalId)
        VALUES (@ProveedorId, GETDATE(), @Total, @UsuarioId, 'ACTIVO', @SucursalIdCalculado);

        DECLARE @CompraId INT = SCOPE_IDENTITY();

        /* ===== Insert Detalle ===== */
        INSERT INTO dbo.tbl_compra_detalle (CompraId, ProductoId, Cantidad, CostoUnitario, Subtotal)
        SELECT
            @CompraId,
            d.ProductoId,
            d.Cantidad,
            d.CostoUnitario,
            CAST(d.Cantidad AS DECIMAL(12,2)) * d.CostoUnitario
        FROM @Detalle d;

        /* ===== Inventario: entrada positiva ===== */
        INSERT INTO dbo.tbl_inventario_movimientos
        (ProductoId, FechaHora, Tipo, Cantidad, CostoUnitario, VentaId, CompraId, ReservaId, Descripcion, UsuarioId, Estado, SucursalId)
        SELECT
            d.ProductoId,
            GETDATE(),
            'COMPRA',
            d.Cantidad,
            d.CostoUnitario,
            NULL,
            @CompraId,
            NULL,
            CONCAT('Entrada por COMPRA. ', ISNULL(@Descripcion,'')),
            @UsuarioId,
            'ACTIVO',
            @SucursalIdCalculado
        FROM @Detalle d;

        /* ===== Caja: egreso ===== */
        INSERT INTO dbo.tbl_caja_movimientos
        (CajaId, FechaHora, Tipo, Concepto, Monto, VentaId, CompraId, UsuarioId, Estado)
        VALUES
        (@CajaId, GETDATE(), 'EGRESO', CONCAT('Compra #', @CompraId), @Total, NULL, @CompraId, @UsuarioId, 'ACTIVO');

        COMMIT;

        SELECT CompraId = @CompraId, Total = @Total;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        THROW;
    END CATCH
END
GO

PRINT 'Stored procedure usp_compra_completa modificado exitosamente';
GO

-- =============================================
-- Modificar usp_reserva_crear_completa
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_reserva_crear_completa]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[usp_reserva_crear_completa];
    PRINT 'Stored procedure usp_reserva_crear_completa eliminado para modificación';
END
GO

CREATE PROCEDURE [dbo].[usp_reserva_crear_completa]
    @ClienteId        INT,
    @UsuarioId        INT,
    @FechaVencimiento DATETIME,
    @Observacion      VARCHAR(255) = NULL,
    @SucursalId INT = NULL,
    @Detalle          dbo.TVP_ReservaDetalle READONLY
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRAN;

        IF NOT EXISTS (SELECT 1 FROM dbo.tbl_clientes WHERE ClienteId=@ClienteId AND Estado='ACTIVO')
            THROW 55001, 'Cliente no válido o inactivo.', 1;

        IF NOT EXISTS (SELECT 1 FROM dbo.tbl_usuarios WHERE UsuarioId=@UsuarioId AND Estado='ACTIVO')
            THROW 55002, 'Usuario no válido o inactivo.', 1;

        IF @FechaVencimiento <= GETDATE()
            THROW 55003, 'FechaVencimiento debe ser futura.', 1;

        IF NOT EXISTS (SELECT 1 FROM @Detalle)
            THROW 55004, 'La reserva debe tener al menos 1 item.', 1;

        /* Validar que todos los ProductoId existan */
        IF EXISTS (
            SELECT 1
            FROM @Detalle d
            LEFT JOIN dbo.tbl_productos p ON p.ProductoId = d.ProductoId
            WHERE p.ProductoId IS NULL
        )
            THROW 55007, 'Hay ProductoId inexistente en el detalle.', 1;

        /* Productos DISPONIBLE */
        IF EXISTS (
            SELECT 1
            FROM @Detalle d
            JOIN dbo.tbl_productos p ON p.ProductoId = d.ProductoId
            WHERE p.Estado <> 'DISPONIBLE'
        )
            THROW 55005, 'Hay productos NO_DISPONIBLE en el detalle.', 1;

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

        /* Validar stock suficiente con lock (anti sobre-reserva) */
        IF EXISTS (
            SELECT 1
            FROM (
                SELECT ProductoId, Req = SUM(CantidadReservada)
                FROM @Detalle
                GROUP BY ProductoId
            ) r
            OUTER APPLY (
                SELECT StockActual = ISNULL(SUM(im.Cantidad), 0)
                FROM dbo.tbl_inventario_movimientos im WITH (UPDLOCK, HOLDLOCK)
                WHERE im.ProductoId = r.ProductoId
                  AND im.Estado = 'ACTIVO'
                  AND (@SucursalIdCalculado IS NULL OR im.SucursalId = @SucursalIdCalculado)
            ) s
            WHERE s.StockActual < r.Req
        )
            THROW 55006, 'Stock insuficiente para reservar uno o más productos.', 1;

        INSERT INTO dbo.tbl_reservas
        (ClienteId, UsuarioId, FechaHora, FechaVencimiento, Estado, Observacion, SucursalId)
        VALUES
        (@ClienteId, @UsuarioId, GETDATE(), @FechaVencimiento, 'ACTIVA', @Observacion, @SucursalIdCalculado);

        DECLARE @ReservaId INT = SCOPE_IDENTITY();

        INSERT INTO dbo.tbl_reserva_detalle (ReservaId, ProductoId, CantidadReservada, Estado)
        SELECT @ReservaId, d.ProductoId, d.CantidadReservada, 'ACTIVO'
        FROM @Detalle d;

        INSERT INTO dbo.tbl_inventario_movimientos
        (ProductoId, FechaHora, Tipo, Cantidad, CostoUnitario, VentaId, CompraId, ReservaId, Descripcion, UsuarioId, Estado, SucursalId)
        SELECT
            d.ProductoId,
            GETDATE(),
            'RESERVA',
            -d.CantidadReservada,
            NULL,
            NULL,
            NULL,
            @ReservaId,
            'Salida por RESERVA',
            @UsuarioId,
            'ACTIVO',
            @SucursalIdCalculado
        FROM @Detalle d;

        COMMIT;

        SELECT ReservaId=@ReservaId, Estado='ACTIVA';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        THROW;
    END CATCH
END
GO

PRINT 'Stored procedure usp_reserva_crear_completa modificado exitosamente';
GO

-- =============================================
-- Modificar usp_caja_abrir
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_caja_abrir]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[usp_caja_abrir];
    PRINT 'Stored procedure usp_caja_abrir eliminado para modificación';
END
GO

CREATE PROCEDURE [dbo].[usp_caja_abrir]
    @SaldoInicial DECIMAL(12,2),
    @UsuarioId INT,
    @SucursalId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRAN;

        IF @SaldoInicial < 0
            THROW 50003, 'SaldoInicial no puede ser negativo.', 1;

        /* Validar usuario */
        IF NOT EXISTS (SELECT 1 FROM dbo.tbl_usuarios WHERE UsuarioId = @UsuarioId AND Estado = 'ACTIVO')
            THROW 50004, 'Usuario no válido o inactivo.', 1;

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

        /* Lock para evitar doble apertura concurrente */
        IF EXISTS (
            SELECT 1
            FROM dbo.tbl_caja WITH (UPDLOCK, HOLDLOCK)
            WHERE Estado = 'ABIERTA'
              AND (@SucursalIdCalculado IS NULL OR SucursalId = @SucursalIdCalculado)
        )
            THROW 50001, 'Ya existe una caja ABIERTA.', 1;

        INSERT INTO dbo.tbl_caja (FechaApertura, SaldoInicial, Estado, SucursalId)
        VALUES (GETDATE(), @SaldoInicial, 'ABIERTA', @SucursalIdCalculado);

        DECLARE @CajaId INT = SCOPE_IDENTITY();

        COMMIT;

        SELECT CajaId = @CajaId;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        THROW;
    END CATCH
END
GO

PRINT 'Stored procedure usp_caja_abrir modificado exitosamente';
GO

PRINT '========================================';
PRINT 'STORED PROCEDURES DE CREACIÓN MODIFICADOS';
PRINT '========================================';
PRINT 'usp_venta_completa: ✓ Modificado';
PRINT 'usp_compra_completa: ✓ Modificado';
PRINT 'usp_reserva_crear_completa: ✓ Modificado';
PRINT 'usp_caja_abrir: ✓ Modificado';
PRINT '========================================';
GO
