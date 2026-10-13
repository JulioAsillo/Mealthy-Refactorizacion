FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Mealthy.Api/Mealthy.Api.csproj", "Mealthy.Api/"]
# Mealthy.Api referencia a ServiceDefaults (Aspire): restore necesita también su .csproj
COPY ["Mealthy.ServiceDefaults/Mealthy.ServiceDefaults.csproj", "Mealthy.ServiceDefaults/"]
RUN dotnet restore "Mealthy.Api/Mealthy.Api.csproj"
COPY . .
WORKDIR "/src/Mealthy.Api"
RUN dotnet build "./Mealthy.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Mealthy.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Mealthy.Api.dll"]
