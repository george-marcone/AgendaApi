-- init.sql: cria banco, tabela Users e popula com 50 registros
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'CoreFlowDb')
BEGIN
	CREATE DATABASE CoreFlowDb;
END
GO

USE CoreFlowDb;
GO

IF OBJECT_ID('dbo.Users') IS NOT NULL
	DROP TABLE dbo.Users;
GO

IF OBJECT_ID('dbo.AuthUsers') IS NOT NULL
	DROP TABLE dbo.AuthUsers;
GO

CREATE TABLE dbo.Users (
	Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	Name NVARCHAR(200) NOT NULL,
	Email NVARCHAR(200) NOT NULL,
	Phone NVARCHAR(50) NOT NULL,
	CreatedAt DATETIMEOFFSET NOT NULL
);
GO

CREATE UNIQUE INDEX IX_Users_Email ON dbo.Users (Email);
GO

CREATE UNIQUE INDEX IX_Users_Phone ON dbo.Users (Phone);
GO

CREATE INDEX IX_Users_CreatedAt ON dbo.Users (CreatedAt);
GO

CREATE TABLE dbo.AuthUsers (
	Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	Name NVARCHAR(200) NOT NULL,
	Email NVARCHAR(200) NOT NULL,
	PasswordHash NVARCHAR(500) NOT NULL,
	CreatedAt DATETIMEOFFSET NOT NULL
);
GO

CREATE UNIQUE INDEX IX_AuthUsers_Email ON dbo.AuthUsers (Email);
GO

INSERT INTO dbo.AuthUsers (Id, Name, Email, PasswordHash, CreatedAt)
VALUES (
	'00000000-0000-0000-0000-000000000101',
	'Admin',
	'admin@coreflow.local',
	'PBKDF2-SHA256.100000.AQIDBAUGBwgJCgsMDQ4PEA==.qcFegJie06o8c1nvLR19oaltyyqxYCeEEOBZYppGVW8=',
	TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')
);
GO

-- Popula com 50 registros
SET NOCOUNT ON;

DECLARE @i INT = 1;
WHILE @i <= 50
BEGIN
	INSERT INTO dbo.Users (Id, Name, Email, Phone, CreatedAt)
	VALUES (
		NEWID(),
		CONCAT('User ', @i),
		CONCAT('user', @i, '@example.com'),
		CONCAT('+55119', RIGHT('00000000' + CAST(@i AS VARCHAR(8)), 8)),
		TODATETIMEOFFSET(DATEADD(MINUTE, @i, SYSUTCDATETIME()), '+00:00')
	);

	SET @i = @i + 1;
END;
GO
