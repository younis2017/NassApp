-- Static PIVOT: Sales by Year (hard-coded 2011-2014)
-- Show total sales by Year on columns and Product Category on rows
SELECT 
    ISNULL(Subcategory, 'Total') AS ProductCategory,
    [2011], [2012], [2013], [2014]
FROM (
    SELECT 
        YEAR(soh.OrderDate) AS OrderYear,
        psc.Name AS Subcategory,
        sod.LineTotal
    FROM Sales.SalesOrderHeader soh
    JOIN Sales.SalesOrderDetail sod ON soh.SalesOrderID = sod.SalesOrderID
    JOIN Production.Product p ON sod.ProductID = p.ProductID
    JOIN Production.ProductSubcategory psc ON p.ProductSubcategoryID = psc.ProductSubcategoryID
) src
PIVOT
(
    SUM(LineTotal)
    FOR OrderYear IN ([2011],[2012],[2013],[2014])
) pvt
ORDER BY CASE WHEN Subcategory IS NULL THEN 1 ELSE 0 END, Subcategory;


-----------------------------------------------------------------------------------------------------------

-- Conditional Aggregation 
SELECT 
    pc.Name AS Category,
    SUM(CASE WHEN YEAR(soh.OrderDate) = 2011 THEN sod.LineTotal END) AS [2011],
    SUM(CASE WHEN YEAR(soh.OrderDate) = 2012 THEN sod.LineTotal END) AS [2012],
    SUM(CASE WHEN YEAR(soh.OrderDate) = 2013 THEN sod.LineTotal END) AS [2013],
    SUM(CASE WHEN YEAR(soh.OrderDate) = 2014 THEN sod.LineTotal END) AS [2014],
    SUM(sod.LineTotal) AS Total
FROM Sales.SalesOrderHeader soh
JOIN Sales.SalesOrderDetail sod ON soh.SalesOrderID = sod.SalesOrderID
JOIN Production.Product p ON sod.ProductID = p.ProductID
JOIN Production.ProductSubcategory psc ON p.ProductSubcategoryID = psc.ProductSubcategoryID
JOIN Production.ProductCategory pc ON psc.ProductCategoryID = pc.ProductCategoryID
--WHERE YEAR(soh.OrderDate) >= 2011
GROUP BY pc.Name
ORDER BY Total DESC;


-----------------------------------------------------------------------------------------------------------

-- Dynamic Pivot - automatically finds all years
DECLARE @columns NVARCHAR(MAX);
DECLARE @sql     NVARCHAR(MAX) = '';

-- Build dynamic list of year columns
SELECT @columns = COALESCE(@columns + ',', '') + QUOTENAME(CAST(OrderYear AS NVARCHAR(4)))
FROM (
    SELECT DISTINCT YEAR(OrderDate) AS OrderYear
    FROM Sales.SalesOrderHeader
    WHERE YEAR(OrderDate) >= 2010
) y
ORDER BY OrderYear;

--SELECT @COLUMNS

-- Build dynamic SQL
SET @sql = '
SELECT 
    Category,
    ' + @columns + ',
    ISNULL(' + REPLACE(@columns, ',', '+') + ',0) AS Total
FROM (
    SELECT 
        pc.Name AS Category,
        YEAR(soh.OrderDate) AS OrderYear,
        sod.LineTotal
    FROM Sales.SalesOrderHeader soh
    JOIN Sales.SalesOrderDetail sod ON soh.SalesOrderID = sod.SalesOrderID
    JOIN Production.Product p ON sod.ProductID = p.ProductID
    JOIN Production.ProductSubcategory psc ON p.ProductSubcategoryID = psc.ProductSubcategoryID
    JOIN Production.ProductCategory pc ON psc.ProductCategoryID = pc.ProductCategoryID
) src
PIVOT (
    SUM(LineTotal)
    FOR OrderYear IN (' + @columns + ')
) pvt
ORDER BY Category;';

--SELECT @sql

EXEC sys.sp_executesql @sql;



