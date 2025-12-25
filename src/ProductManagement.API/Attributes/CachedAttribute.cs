using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ProductManagement.Application.Interfaces;

namespace ProductManagement.API.Attributes;

/// <summary>
/// Attribute for caching controller action results
/// Usage: [Cached(600)] // Cache for 600 seconds
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class CachedAttribute : Attribute, IAsyncActionFilter
{
    private readonly int _durationInSeconds;

    public CachedAttribute(int durationInSeconds = 300)
    {
        _durationInSeconds = durationInSeconds;
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var cacheService = context.HttpContext.RequestServices
            .GetRequiredService<ICacheService>();

        var cacheKey = GenerateCacheKey(context.HttpContext.Request);

        // Try to get from cache
        var cachedResponse = await cacheService.GetAsync<object>(cacheKey);

        if (cachedResponse != null)
        {
            context.Result = new OkObjectResult(cachedResponse);
            return;
        }

        // Execute action
        var executedContext = await next();

        // Cache the result
        if (executedContext.Result is OkObjectResult okResult)
        {
            await cacheService.SetAsync(
                cacheKey,
                okResult.Value,
                TimeSpan.FromSeconds(_durationInSeconds));
        }
    }

    private static string GenerateCacheKey(HttpRequest request)
    {
        var keyBuilder = new StringBuilder();

        keyBuilder.Append($"{request.Path}");

        // Include query parameters
        foreach (var (key, value) in request.Query.OrderBy(x => x.Key))
        {
            keyBuilder.Append($"|{key}-{value}");
        }

        return keyBuilder.ToString();
    }
}