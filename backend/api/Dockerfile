FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ["UrbanSync/UrbanSync.Api.csproj", "UrbanSync/"]
COPY ["UrbanSync.Business/UrbanSync.Business.csproj", "UrbanSync.Business/"]
COPY ["UrbanSync.DataAccess/UrbanSync.DataAccess.csproj", "UrbanSync.DataAccess/"]
COPY ["UrbanSync.Domain/UrbanSync.Domain.csproj", "UrbanSync.Domain/"]

RUN dotnet restore "UrbanSync/UrbanSync.Api.csproj"

COPY . .

WORKDIR "/src/UrbanSync"

RUN dotnet publish "UrbanSync.Api.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:10000

EXPOSE 10000

ENTRYPOINT ["dotnet", "UrbanSync.Api.dll"]