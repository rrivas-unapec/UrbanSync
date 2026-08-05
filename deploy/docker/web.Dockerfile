FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore

WORKDIR /src

COPY global.json ./
COPY Directory.Build.props ./
COPY Directory.Packages.props ./

COPY src/web/UrbanSync.Web/UrbanSync.Web.csproj \
    src/web/UrbanSync.Web/

RUN dotnet restore \
    src/web/UrbanSync.Web/UrbanSync.Web.csproj

FROM restore AS build

COPY src/web ./src/web

RUN dotnet publish \
    src/web/UrbanSync.Web/UrbanSync.Web.csproj \
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

ENTRYPOINT ["dotnet", "UrbanSync.Web.dll"]