-- Script de inicialización completa de la base de datos
-- Crea las tablas y carga datos iniciales

USE ia_proyecto_eventos;

-- ==================================================
-- CREAR TABLAS
-- ==================================================

-- Tabla: TipoEventos
CREATE TABLE IF NOT EXISTS `TipoEventos` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Nombre` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Estado` tinyint(1) NOT NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Tabla: Usuarios
CREATE TABLE IF NOT EXISTS `Usuarios` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Username` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Nombre` varchar(150) CHARACTER SET utf8mb4 NOT NULL DEFAULT '',
    `Telefono` varchar(30) CHARACTER SET utf8mb4 NOT NULL DEFAULT '',
    `FechaNacimiento` date NULL,
    `Genero` int NULL,
    `PasswordHash` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Salt` longtext CHARACTER SET utf8mb4 NULL,
    `Email` varchar(200) CHARACTER SET utf8mb4 NULL,
    `Rol` varchar(50) CHARACTER SET utf8mb4 NOT NULL DEFAULT 'usuario',
    `Estado` tinyint(1) NOT NULL DEFAULT 1,
    `FechaCreacion` datetime(6) NOT NULL,
    `UltimoIngresoUtc` datetime(6) NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_Usuarios_Username` (`Username`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Tabla: Personas
CREATE TABLE IF NOT EXISTS `Personas` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Nombre` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Telefono` longtext CHARACTER SET utf8mb4 NOT NULL,
    `FechaNacimiento` date NOT NULL,
    `Genero` int NOT NULL,
    `Estado` tinyint(1) NOT NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Tabla: Eventos
CREATE TABLE IF NOT EXISTS `Eventos` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Nombre` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Descripcion` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Direccion` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Costo` decimal(18,2) NOT NULL,
    `FechaInicio` date NOT NULL,
    `FechaFin` date NOT NULL,
    `HoraInicio` time(6) NOT NULL,
    `HoraFin` time(6) NOT NULL,
    `TipoEventoId` int NOT NULL,
    `UsuarioId` int NULL,
    `Estado` tinyint(1) NOT NULL,
    PRIMARY KEY (`Id`),
    KEY `IX_Eventos_TipoEventoId` (`TipoEventoId`),
    KEY `IX_Eventos_UsuarioId` (`UsuarioId`),
    CONSTRAINT `FK_Eventos_TipoEventos_TipoEventoId` FOREIGN KEY (`TipoEventoId`) REFERENCES `TipoEventos` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Eventos_Usuarios_UsuarioId` FOREIGN KEY (`UsuarioId`) REFERENCES `Usuarios` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Tabla: RegistroAsistencias
CREATE TABLE IF NOT EXISTS `RegistroAsistencias` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `FechaEntrada` datetime(6) NOT NULL,
    `Observacion` longtext CHARACTER SET utf8mb4 NOT NULL,
    `EventoId` int NOT NULL,
    `PersonaId` int NOT NULL,
    PRIMARY KEY (`Id`),
    KEY `IX_RegistroAsistencias_EventoId` (`EventoId`),
    KEY `IX_RegistroAsistencias_PersonaId` (`PersonaId`),
    CONSTRAINT `FK_RegistroAsistencias_Eventos_EventoId` FOREIGN KEY (`EventoId`) REFERENCES `Eventos` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_RegistroAsistencias_Personas_PersonaId` FOREIGN KEY (`PersonaId`) REFERENCES `Personas` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ==================================================
-- INSERTAR DATOS INICIALES
-- ==================================================

-- Datos iniciales para TipoEventos
INSERT IGNORE INTO TipoEventos (Id, Nombre, Estado) VALUES
(1, 'Conferencia', 1),
(2, 'Taller', 1),
(3, 'Seminario', 1),
(4, 'Networking', 1),
(5, 'Social', 1);

-- Datos iniciales para Usuarios
-- Password para todos: "admin123"
-- Hash: TtyGN64tC+JY94vGL450HD9tDiYgkJ3AYbdVTpp35dY=
-- Salt: maijJG2KUDRr3FIEA6Wtzg==
INSERT IGNORE INTO Usuarios (Id, Username, Nombre, Telefono, FechaNacimiento, Genero, PasswordHash, Salt, Email, Rol, Estado, FechaCreacion) VALUES
(1, 'admin', 'Administrador Sistema', '555-0001', '1990-01-01', 0, 
'TtyGN64tC+JY94vGL450HD9tDiYgkJ3AYbdVTpp35dY=', 'maijJG2KUDRr3FIEA6Wtzg==', 'admin@eventos.com', 'admin', 1, UTC_TIMESTAMP()),
(2, 'usuario1', 'Juan Pérez', '555-0002', '1995-05-15', 0,
'TtyGN64tC+JY94vGL450HD9tDiYgkJ3AYbdVTpp35dY=', 'maijJG2KUDRr3FIEA6Wtzg==', 'juan@eventos.com', 'usuario', 1, UTC_TIMESTAMP()),
(3, 'usuario2', 'María García', '555-0003', '1992-08-20', 1,
'TtyGN64tC+JY94vGL450HD9tDiYgkJ3AYbdVTpp35dY=', 'maijJG2KUDRr3FIEA6Wtzg==', 'maria@eventos.com', 'usuario', 1, UTC_TIMESTAMP());

-- Datos iniciales para Personas
INSERT IGNORE INTO Personas (Id, Nombre, Telefono, FechaNacimiento, Genero, Estado) VALUES
(1, 'Carlos Rodríguez', '555-1001', '1988-03-10', 0, 1),
(2, 'Ana Martínez', '555-1002', '1990-07-25', 1, 1),
(3, 'Luis Fernández', '555-1003', '1985-11-30', 0, 1),
(4, 'Sofia López', '555-1004', '1993-02-14', 1, 1),
(5, 'Diego Sánchez', '555-1005', '1987-09-05', 0, 1);

-- Datos iniciales para Eventos
INSERT IGNORE INTO Eventos (Id, Nombre, Descripcion, Direccion, Costo, FechaInicio, FechaFin, HoraInicio, HoraFin, TipoEventoId, UsuarioId, Estado) VALUES
(1, 'Conferencia de Tecnología 2025', 'Gran evento de tecnología e innovación', 'Centro de Convenciones, Av. Principal 123', 50.00, 
'2025-03-15', '2025-03-15', '09:00:00', '18:00:00', 1, 1, 1),
(2, 'Taller de Desarrollo Web', 'Aprende las últimas tecnologías web', 'Sala de Capacitación, Calle Tech 456', 30.00,
'2025-04-10', '2025-04-10', '10:00:00', '16:00:00', 2, 1, 1),
(3, 'Seminario de IA y Machine Learning', 'Introducción a la Inteligencia Artificial', 'Universidad Tech, Campus Norte', 75.00,
'2025-05-20', '2025-05-21', '08:00:00', '17:00:00', 3, 2, 1),
(4, 'Networking Empresarial', 'Conecta con profesionales del sector', 'Hotel Business Center', 0.00,
'2025-06-05', '2025-06-05', '18:00:00', '22:00:00', 4, 2, 1);

-- Datos iniciales para RegistroAsistencias
INSERT IGNORE INTO RegistroAsistencias (Id, FechaEntrada, Observacion, EventoId, PersonaId) VALUES
(1, UTC_TIMESTAMP(), 'Registro confirmado', 1, 1),
(2, UTC_TIMESTAMP(), 'Registro confirmado', 1, 2),
(3, UTC_TIMESTAMP(), 'Registro confirmado', 2, 3),
(4, UTC_TIMESTAMP(), 'Registro VIP', 3, 4),
(5, UTC_TIMESTAMP(), 'Registro confirmado', 4, 5);

-- ==================================================
-- VERIFICACIÓN
-- ==================================================

SELECT 'Base de datos inicializada correctamente' AS Resultado;
SELECT COUNT(*) AS TipoEventos FROM TipoEventos;
SELECT COUNT(*) AS Usuarios FROM Usuarios;
SELECT COUNT(*) AS Personas FROM Personas;
SELECT COUNT(*) AS Eventos FROM Eventos;
SELECT COUNT(*) AS RegistroAsistencias FROM RegistroAsistencias;
