# See https://aka.ms/customizecontainer to learn how to customize your debug container 
# and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Base image for running the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Fix: Correct paths based on your actual project structure
COPY ["MusicWebAPI/MusicWebAPI.API.csproj", "MusicWebAPI/"]
COPY ["MusicWebAPI.Application/MusicWebAPI.Application.csproj", "MusicWebAPI.Application/"]
COPY ["MusicWebAPI.Infrastructure/MusicWebAPI.Infrastructure.csproj", "MusicWebAPI.Infrastructure/"]
COPY ["MusicWebAPI.Domain/MusicWebAPI.Domain.csproj", "MusicWebAPI.Domain/"]
COPY ["MusicWebAPI.Core/MusicWebAPI.Core.csproj", "MusicWebAPI.Core/"]

# Restore dependencies
RUN dotnet restore "MusicWebAPI/MusicWebAPI.API.csproj"

# Copy all files and build
COPY . .
WORKDIR "/src/MusicWebAPI"
RUN dotnet build "MusicWebAPI.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MusicWebAPI.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MusicWebAPI.API.dll"]
