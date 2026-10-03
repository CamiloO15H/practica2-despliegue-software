# =============================================================
# Práctica 2 · Despliegue de Software · API de Productos (.NET 8)
# Dockerfile multi-etapa: compila con el SDK y ejecuta con el runtime liviano.
# =============================================================

# ---------- Etapa 1: build (imagen con el SDK completo) ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Se copia primero solo el .csproj y se restauran dependencias.
# Así Docker cachea esta capa y no vuelve a descargar paquetes NuGet
# mientras el .csproj no cambie.
COPY src/Practica2.Api/Practica2.Api.csproj src/Practica2.Api/
RUN dotnet restore src/Practica2.Api/Practica2.Api.csproj

# Luego se copia el resto del código y se publica en modo Release.
COPY src/ src/
RUN dotnet publish src/Practica2.Api/Practica2.Api.csproj -c Release -o /app/publish --no-restore

# ---------- Etapa 2: runtime (imagen final, más pequeña) ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Puerto HTTP en el que escucha la API dentro del contenedor.
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# Solo se copian los binarios publicados, no el código fuente ni el SDK.
COPY --from=build /app/publish .

# Se ejecuta con el usuario sin privilegios que trae la imagen oficial de .NET 8.
USER $APP_UID

ENTRYPOINT ["dotnet", "Practica2.Api.dll"]
