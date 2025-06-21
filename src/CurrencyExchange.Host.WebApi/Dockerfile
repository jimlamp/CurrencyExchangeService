# Use official .NET 8 SDK for building the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy everything first (ensure all project files are copied)
COPY . .

# Debugging: List files inside the container
RUN ls -R /app

# Restore dependencies
RUN dotnet restore CurrencyExchange.Host.WebApi.csproj

# Build and publish the project
RUN dotnet publish CurrencyExchange.Host.WebApi.csproj -c Release -o /out

# Use ASP.NET 8 runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /out .

# Expose the application port
EXPOSE 80
CMD ["dotnet", "CurrencyExchange.Host.WebApi.dll"]
