# Dockerfile pour déploiement .NET sur Render

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copier uniquement les fichiers projet d'abord pour restaurer les dépendances
COPY backend.csproj ./
RUN dotnet restore "backend.csproj"

# Copier le reste du code
COPY . ./
RUN dotnet publish "backend.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 5000
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "backend.dll"]
