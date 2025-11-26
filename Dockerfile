FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5055

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/Users.Api/Users.Api.csproj", "src/Users.Api/"]
COPY ["src/Users.Domain/Users.Domain.csproj", "src/Users.Domain/"]
COPY ["src/Users.Application/Users.Application.csproj", "src/Users.Application/"]
COPY ["src/Users.Infra/Users.Infra.csproj", "src/Users.Infra/"]

RUN dotnet restore "./src/Users.Api/Users.Api.csproj"

COPY . .
WORKDIR "/src/src/Users.Api"
RUN dotnet build "./Users.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Users.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Users.Api.dll"]
