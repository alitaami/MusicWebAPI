# ---------- BASE IMAGE ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# ---------- BUILD IMAGE ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ["MusicWebAPI/MusicWebAPI.API.csproj", "MusicWebAPI/"]
COPY ["MusicWebAPI.Application/MusicWebAPI.Application.csproj", "MusicWebAPI.Application/"]
COPY ["MusicWebAPI.Infrastructure/MusicWebAPI.Infrastructure.csproj", "MusicWebAPI.Infrastructure/"]
COPY ["MusicWebAPI.Domain/MusicWebAPI.Domain.csproj", "MusicWebAPI.Domain/"]
COPY ["MusicWebAPI.Core/MusicWebAPI.Core.csproj", "MusicWebAPI.Core/"]

# Restore dependencies for the main project
RUN dotnet restore "MusicWebAPI/MusicWebAPI.API.csproj"

# Copy all remaining source files
COPY . .

# Set working directory to the main project and build it
WORKDIR "/src/MusicWebAPI"
RUN dotnet build "MusicWebAPI.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ---------- PUBLISH IMAGE ----------
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MusicWebAPI.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ---------- FINAL IMAGE ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MusicWebAPI.API.dll"]
