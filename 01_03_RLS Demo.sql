CREATE DATABASE DEMO
GO

USE DEMO
GO

CREATE USER Manager WITHOUT LOGIN;  
CREATE USER SalesUS WITHOUT LOGIN;  
CREATE USER SalesEU WITHOUT LOGIN;  

GO

CREATE TABLE dbo.Sales  
    (  
    OrderID int,  
    SalesRep sysname,  
    CarModel varchar(50),  
    Qty int  
    );  

INSERT INTO dbo.Sales VALUES   
(1, 'SalesUS', 'Ford Focus', 1000),   
(2, 'SalesUS', 'Lincoln Navigator', 10),   
(3, 'SalesUS', 'Ford F350', 500),  
(4, 'SalesEU', 'BMW X5', 100),   
(5, 'SalesEU', 'Audi Q5', 200),   
(6, 'SalesEU', 'Pagani Huayra', 5);  
-- View the 6 rows in the table  
SELECT * FROM Sales;

GRANT SELECT ON Sales TO Manager;  
GRANT SELECT ON Sales TO SalesUS;  
GRANT SELECT ON Sales TO SalesEU;
GO

CREATE OR ALTER FUNCTION dbo.fn_security(@SalesRep AS sysname)  
    RETURNS TABLE  
WITH SCHEMABINDING  
AS  
    RETURN SELECT 1 AS fn_security_result   
WHERE @SalesRep = USER_NAME() OR USER_NAME() = 'Manager';
GO


CREATE SECURITY POLICY SalesFilter  
ADD FILTER PREDICATE dbo.fn_security(SalesRep)   
ON dbo.Sales  
WITH (STATE = ON);



EXECUTE AS USER = 'SalesUS';  
SELECT * FROM Sales;   
REVERT;  

EXECUTE AS USER = 'SalesEU';  
SELECT * FROM Sales;   
REVERT;  

EXECUTE AS USER = 'Manager';  
SELECT * FROM Sales;   
REVERT;

drop SECURITY POLICY SalesFilter
drop table Sales;
drop user Manager;
drop user SalesUS;
drop user SalesEU;
GO