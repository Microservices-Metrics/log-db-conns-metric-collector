# Stage 1: build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY src/LogDbConnsMetricCollector.Api/LogDbConnsMetricCollector.Api.csproj ./src/LogDbConnsMetricCollector.Api/
RUN dotnet restore ./src/LogDbConnsMetricCollector.Api/LogDbConnsMetricCollector.Api.csproj

COPY src/ ./src/
RUN dotnet publish ./src/LogDbConnsMetricCollector.Api/LogDbConnsMetricCollector.Api.csproj \
    -c Release \
    -o /app/publish

# Stage 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Create logs directory and ensure it is writable
RUN mkdir -p /app/logs

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

EXPOSE 8080

ENTRYPOINT ["dotnet", "LogDbConnsMetricCollector.Api.dll"]
