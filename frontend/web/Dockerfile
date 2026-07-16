FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ["UrbanSync.Web/UrbanSync.Web.csproj", "UrbanSync.Web/"]

RUN dotnet restore "UrbanSync.Web/UrbanSync.Web.csproj"

COPY . .

RUN dotnet publish "UrbanSync.Web/UrbanSync.Web.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:10000

EXPOSE 10000

ENTRYPOINT ["dotnet", "UrbanSync.Web.dll"]