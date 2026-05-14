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

-- Popula com 50 registros
SET NOCOUNT ON;

DECLARE @i INT = 1;
WHILE @i <= 50
BEGIN
	INSERT INTO dbo.Users (Id, Name, Email, Phone)
	VALUES (NEWID(), CONCAT('User ', @i), CONCAT('user', @i, '@example.com'), CONCAT('+55 11 9', RIGHT('000000' + CAST((100000 + @i) AS VARCHAR(6)),6)));
	SET @i = @i + 1;
END;
GO
