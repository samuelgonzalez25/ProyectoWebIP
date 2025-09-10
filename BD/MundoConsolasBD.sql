-- MySQL Script: MundoConsolasBD
-- Temática: Venta de Consolas de Videojuegos

SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0;
SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0;
SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION';

-- -----------------------------------------------------
-- Schema MundoConsolasBD
-- -----------------------------------------------------
CREATE SCHEMA IF NOT EXISTS `MundoConsolasBD` DEFAULT CHARACTER SET utf8mb3 ;
USE `MundoConsolasBD` ;

-- -----------------------------------------------------
-- Table Nivel_Academico (se mantiene para empleados)
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `Nivel_Academico` (
  `idNivel` INT NOT NULL AUTO_INCREMENT,
  `Nivel` VARCHAR(50) NOT NULL,
  `Institucion` VARCHAR(100) NULL DEFAULT NULL,
  PRIMARY KEY (`idNivel`)
) ENGINE=InnoDB DEFAULT CHARACTER SET=utf8mb3;

-- -----------------------------------------------------
-- Table Generales (datos personales)
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `Generales` (
  `idGeneral` INT NOT NULL AUTO_INCREMENT,
  `nombre` VARCHAR(50) NOT NULL,
  `apellido` VARCHAR(50) NOT NULL,
  `sexo` ENUM('M', 'F') NOT NULL,
  `Nivel_Academico_idNivel` INT NOT NULL,
  PRIMARY KEY (`idGeneral`),
  INDEX `fk_Generales_Nivel_Academico1_idx` (`Nivel_Academico_idNivel` ASC) VISIBLE,
  CONSTRAINT `fk_Generales_Nivel_Academico1`
    FOREIGN KEY (`Nivel_Academico_idNivel`)
    REFERENCES `Nivel_Academico` (`idNivel`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARACTER SET=utf8mb3;

-- -----------------------------------------------------
-- Table user (usuarios del sistema)
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `user` (
  `idUSER` INT NOT NULL AUTO_INCREMENT,
  `username` VARCHAR(16) NOT NULL,
  `password` VARCHAR(32) NOT NULL,
  `rol` ENUM('ADMIN', 'ENCARGADO', 'EMPLEADO') NOT NULL,
  `Generales_idGeneral` INT NOT NULL,
  PRIMARY KEY (`idUSER`),
  UNIQUE INDEX `UC_User_Username` (`username` ASC) VISIBLE,
  INDEX `fk_user_Generales_idx` (`Generales_idGeneral` ASC) VISIBLE,
  CONSTRAINT `fk_user_Generales`
    FOREIGN KEY (`Generales_idGeneral`)
    REFERENCES `Generales` (`idGeneral`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARACTER SET=utf8mb3;

-- -----------------------------------------------------
-- Table Proveedores
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `Proveedores` (
  `idProveedor` INT NOT NULL AUTO_INCREMENT,
  `Nombre` VARCHAR(100) NOT NULL,
  `Direccion` VARCHAR(255) NULL DEFAULT NULL,
  `Telefono` VARCHAR(20) NULL DEFAULT NULL,
  `Email` VARCHAR(100) NULL DEFAULT NULL,
  PRIMARY KEY (`idProveedor`)
) ENGINE=InnoDB DEFAULT CHARACTER SET=utf8mb3;

-- -----------------------------------------------------
-- Table Consolas (antes GPU)
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `Consolas` (
  `idConsola` INT NOT NULL AUTO_INCREMENT,
  `Marca` VARCHAR(50) NOT NULL,
  `Modelo` VARCHAR(100) NOT NULL,
  `Almacenamiento` VARCHAR(50) NOT NULL,
  `Generacion` VARCHAR(50) NOT NULL,
  `Incluye_Juegos` TINYINT(1) NOT NULL,
  `Imagen` VARCHAR(255) NOT NULL,
  `Precio` DECIMAL(10,2) NOT NULL,
  `Proveedores_idProveedor` INT NOT NULL,
  PRIMARY KEY (`idConsola`),
  INDEX `fk_Consolas_Proveedores1_idx` (`Proveedores_idProveedor` ASC) VISIBLE,
  CONSTRAINT `fk_Consolas_Proveedores1`
    FOREIGN KEY (`Proveedores_idProveedor`)
    REFERENCES `Proveedores` (`idProveedor`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARACTER SET=utf8mb3;

-- -----------------------------------------------------
-- Inserts de ejemplo
-- -----------------------------------------------------

-- Nivel académico
INSERT INTO Nivel_Academico (Nivel, Institucion)
VALUES ('Licenciatura', 'Universidad Nacional');

-- Generales
INSERT INTO Generales (nombre, apellido, sexo, Nivel_Academico_idNivel)
VALUES ('Admin', 'Admin', 'M', 1);

-- Usuario administrador
INSERT INTO user (username, password, rol, Generales_idGeneral)
VALUES ('Admin', 'Admin', 'ADMIN', 1);

-- Proveedores de consolas
INSERT INTO Proveedores (Nombre, Direccion, Telefono, Email) VALUES
('Sony Interactive Entertainment', '2207 Bridgepointe Pkwy, San Mateo, CA, USA', '+1-650-655-8000', 'contact@sony.com'),
('Microsoft Xbox Division', 'One Microsoft Way, Redmond, WA, USA', '+1-425-882-8080', 'xbox@microsoft.com'),
('Nintendo Co., Ltd.', '11-1 Kamitoba-hokotate-cho, Kyoto, Japan', '+81-75-662-9600', 'support@nintendo.com');

-- Consolas
INSERT INTO Consolas (Marca, Modelo, Almacenamiento, Generacion, Incluye_Juegos, Imagen, Precio, Proveedores_idProveedor) VALUES
('Sony', 'PlayStation 5', '825 GB SSD', 'Novena', 1, 'https://m.media-amazon.com/images/I/619BkvKW35L._SL1500_.jpg', 499.99, 1),
('Sony', 'PlayStation 5 Digital Edition', '825 GB SSD', 'Novena', 0, 'https://m.media-amazon.com/images/I/51eOztNd3-L._SL1000_.jpg', 399.99, 1),
('Microsoft', 'Xbox Series X', '1 TB SSD', 'Novena', 1, 'https://m.media-amazon.com/images/I/71NBQ2a52CL._SL1500_.jpg', 499.99, 2),
('Microsoft', 'Xbox Series S', '512 GB SSD', 'Novena', 0, 'https://m.media-amazon.com/images/I/71NBQ2a52CL._SL1500_.jpg', 299.99, 2),
('Nintendo', 'Nintendo Switch OLED', '64 GB interno', 'Octava', 1, 'https://m.media-amazon.com/images/I/61-PblYntsL._SL1500_.jpg', 349.99, 3),
('Nintendo', 'Nintendo Switch Lite', '32 GB interno', 'Octava', 0, 'https://m.media-amazon.com/images/I/71zb1b8P0NL._SL1500_.jpg', 199.99, 3);

-- Consulta ejemplo
SELECT c.*, p.Nombre AS Proveedor
FROM Consolas c
INNER JOIN Proveedores p ON c.Proveedores_idProveedor = p.idProveedor;

SET SQL_MODE=@OLD_SQL_MODE;
SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;
SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS;
