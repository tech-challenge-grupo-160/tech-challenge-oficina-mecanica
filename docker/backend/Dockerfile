FROM mcr.microsoft.com/dotnet/sdk:10.0 AS development
WORKDIR /app

# copy csproj and restore
COPY ["Fiap.TechChallenge.OficinaMecanica.csproj", "./"]
RUN dotnet restore "./Fiap.TechChallenge.OficinaMecanica.csproj"

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
COPY ["Fiap.TechChallenge.OficinaMecanica.csproj", "./"]
RUN dotnet restore "./Fiap.TechChallenge.OficinaMecanica.csproj"

# copy everything and publish
COPY . .
RUN dotnet publish "Fiap.TechChallenge.OficinaMecanica.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Fiap.TechChallenge.OficinaMecanica.dll"]
