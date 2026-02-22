#!/bin/bash

echo "Waiting for SQL Server to be ready..."
sleep 30

echo "Running database migrations..."

# Run migrations
dotnet ef database update --project /src/ProductManagement.Infrastructure/ProductManagement.Infrastructure.csproj --startup-project /src/ProductManagement.API/ProductManagement.API.csproj

echo "Database migrations completed!"