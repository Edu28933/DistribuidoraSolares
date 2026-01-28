-- =============================================
-- DEJAR SOLO PANTALLAS POR MÓDULO (Index)
-- En permisos hay UNA fila por módulo; las columnas
-- Puede Ver/Crear/Editar/Eliminar controlan las acciones.
-- Este script inactiva las pantallas "Crear", "Editar", etc.
-- =============================================

USE [bd_distribuidora_solares];
GO

-- Inactivar todas las pantallas que NO son la principal del módulo (Index)
UPDATE tbl_pantallas
SET Estado = 'INACTIVO'
WHERE Estado = 'ACTIVO'
  AND Accion IS NOT NULL
  AND Accion <> 'Index';

PRINT 'Pantallas no principales (Create, Edit, etc.) pasaron a INACTIVO.';
PRINT 'La gestión de permisos mostrará solo una fila por módulo.';
GO
