-- Stored Procedure สำหรับ Complex Query
-- Best Practice: ใช้ stored procedure สำหรับ queries ที่ซับซ้อนและใช้บ่อย

CREATE OR ALTER PROCEDURE sp_GetTopSellingProducts
    @TopCount INT = 10,
    @StartDate DATETIME2 = NULL,
    @EndDate DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Set default dates if not provided
    IF @StartDate IS NULL
        SET @StartDate = DATEADD(MONTH, -1, GETUTCDATE());

    IF @EndDate IS NULL
        SET @EndDate = GETUTCDATE();

    -- CTE for better readability
    ;WITH ProductSales AS (
        SELECT
            p.Id AS ProductId,
            p.Name AS ProductName,
            p.SKU,
            -- Assuming we have an Orders table (simplified for demo)
            COUNT(DISTINCT o.Id) AS TotalOrders,
            SUM(oi.Quantity) AS TotalSold,
            SUM(oi.Quantity * oi.UnitPrice) AS TotalRevenue
        FROM Products p
        INNER JOIN OrderItems oi ON p.Id = oi.ProductId
        INNER JOIN Orders o ON oi.OrderId = o.Id
        WHERE
            o.CreatedAt >= @StartDate
            AND o.CreatedAt <= @EndDate
            AND p.IsDeleted = 0
            AND o.Status != 'Cancelled'
        GROUP BY p.Id, p.Name, p.SKU
    )
    SELECT TOP (@TopCount)
        ProductId,
        ProductName,
        SKU,
        TotalSold,
        TotalRevenue,
        CAST(TotalRevenue / NULLIF(TotalSold, 0) AS DECIMAL(18, 2)) AS AverageOrderValue
    FROM ProductSales
    ORDER BY TotalRevenue DESC, TotalSold DESC;
END;
GO

-- Performance: Create indexes for better query performance
CREATE NONCLUSTERED INDEX IX_Orders_CreatedAt_Status
ON Orders (CreatedAt, Status)
INCLUDE (Id)
WHERE IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_OrderItems_ProductId_OrderId
ON OrderItems (ProductId, OrderId)
INCLUDE (Quantity, UnitPrice);