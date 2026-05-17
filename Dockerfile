# ── ETAPA 1: Compilar el Backend (.NET) ────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-net
WORKDIR /src

# Copiar archivos de proyecto
COPY ["Microservicio.Clientes.Api/Microservicio.Clientes.Api.csproj", "Microservicio.Clientes.Api/"]
COPY ["Microservicio.Clientes.Business/Microservicio.Clientes.Business.csproj", "Microservicio.Clientes.Business/"]
COPY ["Microservicio.Clientes.DataManagement/Microservicio.Clientes.DataManagement.csproj", "Microservicio.Clientes.DataManagement/"]
COPY ["Microservicio.Cliente.DatAccess/Microservicio.Cliente.DatAccess.csproj", "Microservicio.Cliente.DatAccess/"]

RUN dotnet restore "Microservicio.Clientes.Api/Microservicio.Clientes.Api.csproj"

# Copiar todo el código y compilar
COPY . .
WORKDIR "/src/Microservicio.Clientes.Api"

RUN dotnet publish "Microservicio.Clientes.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ── ETAPA 2: Runtime Final ────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build-net /app/publish .

# Render usa el puerto proporcionado en la variable de entorno PORT, por defecto 10000
ENV ASPNETCORE_URLS=http://+:${PORT:-10000}
EXPOSE 10000

ENTRYPOINT ["dotnet", "Microservicio.Clientes.Api.dll"]
