using Microsoft.Extensions.Logging;
using ProductManagement.Domain.Interfaces;

namespace ProductManagement.Application.BackgroundJobs;

/// <summary>
/// Background job to check low stock and send alerts
/// </summary>
public class ProductStockAlertJob
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProductStockAlertJob> _logger;

    public ProductStockAlertJob(
        IUnitOfWork unitOfWork,
        ILogger<ProductStockAlertJob> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Execute()
    {
        _logger.LogInformation("Executing Product Stock Alert Job at {Time}", DateTime.UtcNow);

        try
        {
            var lowStockProducts = await _unitOfWork.Products.GetLowStockProductsAsync();
            var productList = lowStockProducts.ToList();

            _logger.LogInformation("Found {Count} low stock products", productList.Count);

            foreach (var product in productList)
            {
                _logger.LogWarning(
                    "Low stock alert - Product: {ProductName} (SKU: {SKU}), Stock: {Stock}, Minimum: {Minimum}",
                    product.Name,
                    product.SKU,
                    product.StockQuantity,
                    product.MinimumStockLevel);

                // TODO: Send email notification
                // await _emailService.SendLowStockAlertAsync(product);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing Product Stock Alert Job");
            throw;
        }
    }
}