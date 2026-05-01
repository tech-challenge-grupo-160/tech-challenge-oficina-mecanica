FROM mcr.microsoft.com/dotnet/sdk:10.0 AS development
WORKDIR /app

# copy csproj and restore
COPY ["Fiap.TechChallenge.OficinaMecanica.Api.csproj", "./"]
RUN dotnet restore "./Fiap.TechChallenge.OficinaMecanica.Api.csproj"

# copy everything
COPY . .

# Enable hot reload
ENV DOTNET_WATCH_RESTART_ON_RUDE_EDIT=true

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "watch", "run", "--urls", "http://+:8080"]


FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# copy csproj and restore
COPY ["Fiap.TechChallenge.OficinaMecanica.Api.csproj", "./"]
RUN dotnet restore "./Fiap.TechChallenge.OficinaMecanica.Api.csproj"

# copy everything and publish
COPY . .
RUN dotnet publish "Fiap.TechChallenge.OficinaMecanica.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
RUN apt-get update && apt-get install -y wget libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Fiap.TechChallenge.OficinaMecanica.Api.dll"]
