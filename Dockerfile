FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["English_games/English_games.csproj", "English_games/"]
RUN dotnet restore "English_games/English_games.csproj"
COPY . .
WORKDIR "/src/English_games"
RUN dotnet build "English_games.csproj" -c Release -o /app/build

FROM build AS publish
COPY ["English_games/interHouse.sqlite", "/app/publish/interHouse.sqlite"]
RUN dotnet publish "English_games.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "English_games.dll"]
