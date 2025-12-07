# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar el archivo csproj y restaurar dependencias
COPY ["IaProyectoEventos.csproj", "./"]
RUN dotnet restore "IaProyectoEventos.csproj"

# Copiar el resto del código y compilar
COPY . .
RUN dotnet build "IaProyectoEventos.csproj" -c Release -o /app/build

# Etapa 2: Publish
FROM build AS publish
RUN dotnet publish "IaProyectoEventos.csproj" -c Release -o /app/publish

# Etapa 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copiar archivos publicados
COPY --from=publish /app/publish .

# Punto de entrada
ENTRYPOINT ["dotnet", "IaProyectoEventos.dll"]
