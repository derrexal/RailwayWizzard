#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 5178

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RailwayWizzard.App/RailwayWizzard.App.csproj", "RailwayWizzard.App/"]
COPY ["RailwayWizzard.Core/RailwayWizzard.Core.csproj", "RailwayWizzard.Core/"]
COPY ["RailwayWizzard.Robot/RailwayWizzard.Robot.csproj", "RailwayWizzard.Robot/"]
RUN dotnet restore "./RailwayWizzard.App/./RailwayWizzard.App.csproj"
COPY . .
WORKDIR "/src/RailwayWizzard.App"
RUN dotnet build "./RailwayWizzard.App.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RailwayWizzard.App.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RailwayWizzard.App.dll"]