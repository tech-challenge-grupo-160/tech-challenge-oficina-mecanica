FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj and restore
COPY ["oficina-mecanica.csproj", "./"]
RUN dotnet restore "./oficina-mecanica.csproj"

# copy everything and publish
COPY . .
RUN dotnet publish "oficina-mecanica.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "oficina-mecanica.dll"]
