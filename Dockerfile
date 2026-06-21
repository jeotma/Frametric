# Frametric — Cinematic Analytics Platform
# Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /app

# Copy csproj files and restore dependencies
COPY backend/*.sln ./backend/
COPY backend/Frametric.Domain/*.csproj ./backend/Frametric.Domain/
COPY backend/Frametric.Application/*.csproj ./backend/Frametric.Application/
COPY backend/Frametric.Infrastructure/*.csproj ./backend/Frametric.Infrastructure/
COPY backend/Frametric.Api/*.csproj ./backend/Frametric.Api/
RUN dotnet restore ./backend/Frametric.Api/Frametric.Api.csproj

# Copy everything else and build release
COPY backend/ ./backend/
RUN dotnet publish ./backend/Frametric.Api/Frametric.Api.csproj -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build-env /app/out .

# Expose Render default port
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Frametric.Api.dll"]
