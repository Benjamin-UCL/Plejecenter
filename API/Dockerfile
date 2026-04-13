FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["API/API.csproj", "API/"]
COPY ["ModelsLibrary/ModelsLibrary.csproj", "ModelsLibrary/"]
RUN dotnet restore "API/API.csproj"
COPY . .
RUN dotnet publish "API/API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "API.dll"]