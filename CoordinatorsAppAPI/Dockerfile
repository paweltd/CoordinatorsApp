# Etap bazowy - obraz do uruchamiania aplikacji w produkcji na Linuxie
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Etap budowania - obraz SDK do kompilacji aplikacji
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Kopiowanie pliku projektu
COPY ["CoordinatorsAppAPI.csproj", "./"]
RUN dotnet restore "CoordinatorsAppAPI.csproj"

# Kopiowanie pozostałych plików
COPY . . 
WORKDIR "/src"
RUN dotnet build "CoordinatorsAppAPI.csproj" -c ${BUILD_CONFIGURATION} -o /app/build

# Etap publikacji - przygotowanie aplikacji do uruchomienia
FROM build AS publish
RUN dotnet publish "CoordinatorsAppAPI.csproj" -c ${BUILD_CONFIGURATION} -o /app/publish /p:UseAppHost=false

# Etap finalny - tworzymy obraz produkcyjny
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CoordinatorsAppAPI.dll"]

# Dodanie zmiennej środowiskowej dla środowiska produkcyjnego
ENV ASPNETCORE_ENVIRONMENT=Production
