FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /src
COPY ["AE.Market.API/AE.Market.API.csproj", "AE.Market.API/"]
COPY ["Application/AE.Market.Application.csproj", "Application/"]
COPY ["Domain/AE.Market.Domain.csproj", "Domain/"]
COPY ["AE.Market.Infrastructure/AE.Market.Infrastructure.csproj", "AE.Market.Infrastructure/"]
RUN dotnet restore "./AE.Market.API/AE.Market.API.csproj"

FROM restore AS build
ARG BUILD_CONFIGURATION=Release
COPY . .
WORKDIR "/src/AE.Market.API"
RUN dotnet build "./AE.Market.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AE.Market.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AE.Market.API.dll"]
