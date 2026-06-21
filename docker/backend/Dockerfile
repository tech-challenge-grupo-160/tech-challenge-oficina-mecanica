FROM mcr.microsoft.com/dotnet/sdk:10.0 AS development
WORKDIR /app

# copy csproj and restore
COPY ["src/API/Fiap.TechChallenge.OficinaMecanica.Api.csproj", "src/API/"]
COPY ["src/Application/Fiap.TechChallenge.OficinaMecanica.Application.csproj", "src/Application/"]
COPY ["src/Domain/Fiap.TechChallenge.OficinaMecanica.Domain.csproj", "src/Domain/"]
COPY ["src/Infrastructure/Fiap.TechChallenge.OficinaMecanica.Infrastructure.csproj", "src/Infrastructure/"]
RUN dotnet restore "src/API/Fiap.TechChallenge.OficinaMecanica.Api.csproj"

# copy everything
COPY . .

# Enable hot reload
ENV DOTNET_WATCH_RESTART_ON_RUDE_EDIT=true

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "watch", "run", "--project", "src/API/Fiap.TechChallenge.OficinaMecanica.Api.csproj", "--urls", "http://+:8080"]


FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# copy csproj and restore
COPY ["src/API/Fiap.TechChallenge.OficinaMecanica.Api.csproj", "src/API/"]
COPY ["src/Application/Fiap.TechChallenge.OficinaMecanica.Application.csproj", "src/Application/"]
COPY ["src/Domain/Fiap.TechChallenge.OficinaMecanica.Domain.csproj", "src/Domain/"]
COPY ["src/Infrastructure/Fiap.TechChallenge.OficinaMecanica.Infrastructure.csproj", "src/Infrastructure/"]
RUN dotnet restore "src/API/Fiap.TechChallenge.OficinaMecanica.Api.csproj"

# copy everything and publish
COPY . .
RUN dotnet publish "src/API/Fiap.TechChallenge.OficinaMecanica.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build --chown=app:app /app/publish .

USER app
ENTRYPOINT ["dotnet", "Fiap.TechChallenge.OficinaMecanica.Api.dll"]
