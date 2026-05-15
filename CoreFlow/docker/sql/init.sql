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

CREATE TABLE dbo.Users (
	Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	Name NVARCHAR(200) NOT NULL,
	Email NVARCHAR(200) NOT NULL,
	Phone NVARCHAR(50) NOT NULL
);
GO

CREATE UNIQUE INDEX IX_Users_Email ON dbo.Users (Email);
GO

CREATE UNIQUE INDEX IX_Users_Phone ON dbo.Users (Phone);
GO

-- Popula com 50 registros
SET NOCOUNT ON;

DECLARE @i INT = 1;
WHILE @i <= 50
BEGIN
	INSERT INTO dbo.Users (Id, Name, Email, Phone)
	VALUES (NEWID(), CONCAT('User ', @i), CONCAT('user', @i, '@example.com'), CONCAT('+55119', RIGHT('00000000' + CAST(@i AS VARCHAR(8)), 8)));
	SET @i = @i + 1;
END;
GO
