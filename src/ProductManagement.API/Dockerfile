# ========================================
# Multi-stage Dockerfile for .NET 8
# Best Practice: Separate build and runtime stages
# ========================================

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["ProductManagement.sln", "./"]
COPY ["src/ProductManagement.API/ProductManagement.API.csproj", "src/ProductManagement.API/"]
COPY ["src/ProductManagement.Application/ProductManagement.Application.csproj", "src/ProductManagement.Application/"]
COPY ["src/ProductManagement.Domain/ProductManagement.Domain.csproj", "src/ProductManagement.Domain/"]
COPY ["src/ProductManagement.Infrastructure/ProductManagement.Infrastructure.csproj", "src/ProductManagement.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/ProductManagement.API/ProductManagement.API.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/src/ProductManagement.API"
RUN dotnet build "ProductManagement.API.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "ProductManagement.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Create non-root user for security
RUN addgroup --system --gid 1000 appgroup \
    && adduser --system --uid 1000 --ingroup appgroup --shell /bin/sh appuser

# Copy published files
COPY --from=publish /app/publish .

# Create logs directory and set permissions
RUN mkdir -p /app/logs && chown -R appuser:appgroup /app

# Switch to non-root user
USER appuser

# Expose ports
EXPOSE 8080
EXPOSE 8081

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "ProductManagement.API.dll"]