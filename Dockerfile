# Etapa 1: build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar todo y restaurar dependencias
COPY . .
RUN dotnet restore "./PuntoTres/PuntoTres.csproj"

# Compilar en Release
RUN dotnet publish "./PuntoTres/PuntoTres.csproj" -c Release -o /app/publish

# Etapa 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Exponer puerto (Render setea PORT dinámico, no hardcodees 5000)
ENV ASPNETCORE_URLS=http://+:$PORT
ENTRYPOINT ["dotnet", "PuntoTres.dll"]
