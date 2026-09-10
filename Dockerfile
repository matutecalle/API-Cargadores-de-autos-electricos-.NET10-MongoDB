# ---------- Etapa 1: compilar y publicar ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restaurar primero: esta capa se cachea mientras no cambie el .csproj
COPY APICargadores.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish APICargadores.csproj -c Release -o /app --no-restore

# ---------- Etapa 2: imagen final, solo con el runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app ./

# La imagen de ASP.NET escucha en el puerto 8080 por defecto
EXPOSE 8080
ENTRYPOINT ["dotnet", "APICargadores.dll"]
