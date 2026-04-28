FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Plejecenter.Api/Plejecenter.Api.csproj", "Plejecenter.Api/"]
COPY ["Plejecenter.Domain/Plejecenter.Domain.csproj", "Plejecenter.Domain/"]
RUN dotnet restore "Plejecenter.Api/Plejecenter.Api.csproj"
COPY . .
RUN dotnet publish "Plejecenter.Api/Plejecenter.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Plejecenter.Api.dll"]