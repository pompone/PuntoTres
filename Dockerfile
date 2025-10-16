# =========================
# Etapa 1: Build
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar solución / proyecto y restaurar
COPY . .
RUN dotnet restore "./PuntoTres/PuntoTres.csproj"

# Publicar en Release a carpeta /app/publish
RUN dotnet publish "./PuntoTres/PuntoTres.csproj" -c Release -o /app/publish

# =========================
# Etapa 2: Runtime
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copiar artefactos publicados
COPY --from=build /app/publish .

# Render (servicio Docker) espera el puerto 10000 dentro del contenedor
ENV ASPNETCORE_URLS=http://0.0.0.0:10000
EXPOSE 10000

# Entrypoint
ENTRYPOINT ["dotnet", "PuntoTres.dll"]

