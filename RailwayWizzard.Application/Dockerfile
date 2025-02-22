#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 5178

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RailwayWizzard.Application/RailwayWizzard.Application.csproj", "RailwayWizzard.Application/"]
COPY ["RailwayWizzard.Rzd.ApiClient/RailwayWizzard.Rzd.ApiClient.csproj", "RailwayWizzard.Rzd.ApiClient/"]
COPY ["RailwayWizzard.Core/RailwayWizzard.Core.csproj", "RailwayWizzard.Core/"]
COPY ["RailwayWizzard.EntityFramework/RailwayWizzard.Infrastructure.csproj", "RailwayWizzard.EntityFramework/"]
COPY ["RailwayWizzard.Robot/RailwayWizzard.Rzd.DataEngin.csproj", "RailwayWizzard.Robot/"]
COPY ["RailwayWizzard.Common/RailwayWizzard.Common.csproj", "RailwayWizzard.Common/"]
RUN dotnet restore "./RailwayWizzard.Application/./RailwayWizzard.Application.csproj"
COPY . .
WORKDIR "/src/RailwayWizzard.Application"
RUN dotnet build "./RailwayWizzard.Application.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RailwayWizzard.Application.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RailwayWizzard.Application.dll"]