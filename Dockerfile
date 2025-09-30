FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["WinterArcApi/WinterArcApi.csproj", "WinterArcApi/"]
RUN dotnet restore "WinterArcApi/WinterArcApi.csproj"
COPY . .
WORKDIR "/src/WinterArcApi"
RUN dotnet build "WinterArcApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WinterArcApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WinterArcApi.dll"]
