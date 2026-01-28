-- Script para actualizar el CHECK constraint de tbl_reserva_detalle
-- para permitir el estado 'OCUPADO' además de 'ACTIVO' y 'CANCELADO'

USE [bd_distribuidora_solares];
GO

-- Eliminar el constraint existente
DECLARE @ConstraintName NVARCHAR(200);
SELECT @ConstraintName = name
FROM sys.check_constraints
WHERE parent_object_id = OBJECT_ID('dbo.tbl_reserva_detalle')
  AND definition LIKE '%Estado%'
  AND definition LIKE '%CANCELADO%';

IF @ConstraintName IS NOT NULL
BEGIN
    EXEC('ALTER TABLE dbo.tbl_reserva_detalle DROP CONSTRAINT ' + @ConstraintName);
    PRINT 'Constraint eliminado: ' + @ConstraintName;
END
ELSE
BEGIN
    PRINT 'No se encontró el constraint a eliminar';
END
GO

-- Agregar el nuevo constraint con 'OCUPADO'
ALTER TABLE dbo.tbl_reserva_detalle
ADD CONSTRAINT CK_tbl_reserva_detalle_Estado 
CHECK ([Estado]='CANCELADO' OR [Estado]='ACTIVO' OR [Estado]='OCUPADO');
GO

PRINT 'Constraint actualizado exitosamente. Estados permitidos: ACTIVO, CANCELADO, OCUPADO';
GO

-- Verificar que el constraint se aplicó correctamente
SELECT 
    name AS ConstraintName,
    definition AS ConstraintDefinition
FROM sys.check_constraints
WHERE parent_object_id = OBJECT_ID('dbo.tbl_reserva_detalle')
  AND name = 'CK_tbl_reserva_detalle_Estado';
GO
