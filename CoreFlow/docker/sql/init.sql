-- init.sql: cria banco, tabela Users e popula com usuario de autenticacao e 50 registros
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

CREATE TABLE dbo.Users (
	Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	Name NVARCHAR(200) NOT NULL,
	Email NVARCHAR(200) NOT NULL,
	Phone NVARCHAR(50) NOT NULL,
	PasswordHash NVARCHAR(500) NOT NULL,
	CreatedAt DATETIMEOFFSET NOT NULL,
	UpdatedAt DATETIMEOFFSET NOT NULL
);
GO

CREATE UNIQUE INDEX IX_Users_Email ON dbo.Users (Email);
GO

CREATE UNIQUE INDEX IX_Users_Phone ON dbo.Users (Phone);
GO

CREATE INDEX IX_Users_CreatedAt ON dbo.Users (CreatedAt);
GO

CREATE INDEX IX_Users_UpdatedAt ON dbo.Users (UpdatedAt);
GO

DECLARE @SeededPasswordHash NVARCHAR(500) = N'PBKDF2-SHA256.100000.AQIDBAUGBwgJCgsMDQ4PEA==.qcFegJie06o8c1nvLR19oaltyyqxYCeEEOBZYppGVW8=';
DECLARE @Now DATETIMEOFFSET = TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00');

INSERT INTO dbo.Users (Id, Name, Email, Phone, PasswordHash, CreatedAt, UpdatedAt)
VALUES (
	'00000000-0000-0000-0000-000000000101',
	'Admin',
	'admin@coreflow.local',
	'+5511900000000',
	@SeededPasswordHash,
	@Now,
	@Now
);

-- Popula com 50 registros
SET NOCOUNT ON;

DECLARE @i INT = 1;
DECLARE @CreatedAt DATETIMEOFFSET;
WHILE @i <= 50
BEGIN
	SET @CreatedAt = TODATETIMEOFFSET(DATEADD(MINUTE, -@i, SYSUTCDATETIME()), '+00:00');

	INSERT INTO dbo.Users (Id, Name, Email, Phone, PasswordHash, CreatedAt, UpdatedAt)
	VALUES (
		NEWID(),
		CONCAT('User ', @i),
		CONCAT('user', @i, '@example.com'),
		CONCAT('+55119', RIGHT('00000000' + CAST(@i AS VARCHAR(8)), 8)),
		@SeededPasswordHash,
		@CreatedAt,
		@CreatedAt
	);

	SET @i = @i + 1;
END;
GO
