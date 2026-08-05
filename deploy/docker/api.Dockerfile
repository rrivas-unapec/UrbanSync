FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore

WORKDIR /src

COPY global.json ./
COPY Directory.Build.props ./
COPY Directory.Packages.props ./

COPY src/backend/UrbanSync.Domain/UrbanSync.Domain.csproj \
    src/backend/UrbanSync.Domain/

COPY src/backend/UrbanSync.Application/UrbanSync.Application.csproj \
    src/backend/UrbanSync.Application/

COPY src/backend/UrbanSync.Infrastructure/UrbanSync.Infrastructure.csproj \
    src/backend/UrbanSync.Infrastructure/

COPY src/backend/UrbanSync.Api/UrbanSync.Api.csproj \
    src/backend/UrbanSync.Api/

RUN dotnet restore \
    src/backend/UrbanSync.Api/UrbanSync.Api.csproj

FROM restore AS build

COPY src/backend ./src/backend

RUN dotnet publish \
    src/backend/UrbanSync.Api/UrbanSync.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

RUN apt-get update \
    && apt-get install \
        --yes \
        --no-install-recommends \
        curl \
    && rm -rf /var/lib/apt/lists/*

RUN addgroup \
        --system \
        --gid 10001 \
        urbansync \
    && adduser \
        --system \
        --uid 10001 \
        --ingroup urbansync \
        urbansync

COPY --from=build \
    --chown=urbansync:urbansync \
    /app/publish .

USER urbansync

ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_EnableDiagnostics=0

EXPOSE 8080

ENTRYPOINT ["dotnet", "UrbanSync.Api.dll"]