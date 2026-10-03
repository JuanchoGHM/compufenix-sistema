CREATE DATABASE IF NOT EXISTS compufenix
  CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE compufenix;

CREATE TABLE usuarios (
  id_usuario      INT AUTO_INCREMENT PRIMARY KEY,
  nombre          VARCHAR(100) NOT NULL,
  correo          VARCHAR(120) NOT NULL UNIQUE,
  contrasena_hash VARCHAR(255) NOT NULL,
  rol             ENUM('Administrador','Tecnico') NOT NULL,
  activo          TINYINT(1) NOT NULL DEFAULT 1
);

CREATE TABLE clientes (
  id_cliente INT AUTO_INCREMENT PRIMARY KEY,
  nombre     VARCHAR(100) NOT NULL,
  telefono   VARCHAR(20),
  correo     VARCHAR(120),
  direccion  VARCHAR(255)
);

CREATE TABLE equipos (
  id_equipo    INT AUTO_INCREMENT PRIMARY KEY,
  id_cliente   INT NOT NULL,
  tipo         VARCHAR(50) NOT NULL,
  marca        VARCHAR(50),
  modelo       VARCHAR(80),
  numero_serie VARCHAR(80),
  FOREIGN KEY (id_cliente) REFERENCES clientes(id_cliente)
);

CREATE TABLE productos (
  id_producto     INT AUTO_INCREMENT PRIMARY KEY,
  nombre          VARCHAR(120) NOT NULL,
  categoria       VARCHAR(60),
  stock_actual    INT NOT NULL DEFAULT 0,
  stock_minimo    INT NOT NULL DEFAULT 0,
  precio_unitario DECIMAL(10,2) NOT NULL DEFAULT 0
);

CREATE TABLE tickets (
  id_ticket     INT AUTO_INCREMENT PRIMARY KEY,
  id_equipo     INT NOT NULL,
  id_tecnico    INT NULL,
  fecha_ingreso DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  estado        ENUM('Recibido','EnDiagnostico','EsperandoRepuesto',
                     'EnReparacion','Reparado','Entregado','Cancelado')
                NOT NULL DEFAULT 'Recibido',
  diagnostico   TEXT,
  costo_total   DECIMAL(10,2) NOT NULL DEFAULT 0,
  FOREIGN KEY (id_equipo)  REFERENCES equipos(id_equipo),
  FOREIGN KEY (id_tecnico) REFERENCES usuarios(id_usuario)
);

CREATE TABLE movimientos_inventario (
  id_movimiento INT AUTO_INCREMENT PRIMARY KEY,
  id_producto   INT NOT NULL,
  id_ticket     INT NULL,
  tipo          ENUM('Entrada','Salida') NOT NULL,
  cantidad      INT NOT NULL CHECK (cantidad > 0),
  fecha         DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (id_producto) REFERENCES productos(id_producto),
  FOREIGN KEY (id_ticket)   REFERENCES tickets(id_ticket)
);

ALTER TABLE compufenix.productos
ADD COLUMN fecha_incorporacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP;