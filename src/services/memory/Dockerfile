# syntax=docker/dockerfile:1.7
# Multi-stage build for Takumi OS Memory Service.
# Targets .NET 9, AKS linux/amd64. Trivy scans this image in CI.
ARG DOTNET_VERSION=9.0

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src

# Copy only the project files first to leverage Docker layer caching
# of `dotnet restore`. Source files are copied in the next stage.
COPY Directory.Build.props Directory.Packages.props global.json ./
COPY src/services/memory/Takumi.Memory.Domain/Takumi.Memory.Domain.csproj src/services/memory/Takumi.Memory.Domain/
COPY src/services/memory/Takumi.Memory.Application/Takumi.Memory.Application.csproj src/services/memory/Takumi.Memory.Application/
COPY src/services/memory/Takumi.Memory.Infrastructure/Takumi.Memory.Infrastructure.csproj src/services/memory/Takumi.Memory.Infrastructure/
COPY src/services/memory/Takumi.Memory.Api/Takumi.Memory.Api.csproj src/services/memory/Takumi.Memory.Api/
RUN dotnet restore src/services/memory/Takumi.Memory.Api/Takumi.Memory.Api.csproj

# Now copy the rest of the source and publish
COPY src/ src/
RUN dotnet publish src/services/memory/Takumi.Memory.Api/Takumi.Memory.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS runtime
WORKDIR /app

# Non-root user, read-only filesystem friendly
RUN groupadd --system --gid 10001 takumi \
    && useradd --system --uid 10001 --gid takumi takumi

COPY --from=build /app/publish ./
RUN chown -R takumi:takumi /app
USER takumi

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_NOLOGO=true

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/healthz || exit 1

ENTRYPOINT ["dotnet", "Takumi.Memory.Api.dll"]
