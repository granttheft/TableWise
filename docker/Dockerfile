FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000
# HEALTHCHECK için curl gerekli (aspnet runtime imajında yok)
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Tablewise.Api/Tablewise.Api.csproj", "src/Tablewise.Api/"]
COPY ["src/Tablewise.Application/Tablewise.Application.csproj", "src/Tablewise.Application/"]
COPY ["src/Tablewise.Domain/Tablewise.Domain.csproj", "src/Tablewise.Domain/"]
COPY ["src/Tablewise.Infrastructure/Tablewise.Infrastructure.csproj", "src/Tablewise.Infrastructure/"]
COPY ["src/Tablewise.RuleEngine/Tablewise.RuleEngine.csproj", "src/Tablewise.RuleEngine/"]
COPY ["Directory.Build.props", "./"]
RUN dotnet restore "src/Tablewise.Api/Tablewise.Api.csproj"
COPY . .
WORKDIR "/src/src/Tablewise.Api"
RUN dotnet build "Tablewise.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Tablewise.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:5000/health/live || exit 1
ENTRYPOINT ["dotnet", "Tablewise.Api.dll"]
