# Build from the repository root: docker build -f src/WholesaleHub.Api/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first so the layer is cached when only source files change.
COPY Directory.Build.props .editorconfig ./
COPY src/WholesaleHub.Domain/*.csproj src/WholesaleHub.Domain/
COPY src/WholesaleHub.Application/*.csproj src/WholesaleHub.Application/
COPY src/WholesaleHub.Infrastructure/*.csproj src/WholesaleHub.Infrastructure/
COPY src/WholesaleHub.Api/*.csproj src/WholesaleHub.Api/
RUN dotnet restore src/WholesaleHub.Api/WholesaleHub.Api.csproj

COPY src/ src/
RUN dotnet publish src/WholesaleHub.Api/WholesaleHub.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "WholesaleHub.Api.dll"]
