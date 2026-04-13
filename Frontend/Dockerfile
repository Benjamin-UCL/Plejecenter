FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Frontend/Frontend.csproj", "Frontend/"]
COPY ["ModelsLibrary/ModelsLibrary.csproj", "ModelsLibrary/"]
RUN dotnet restore "Frontend/Frontend.csproj"
COPY . .
RUN dotnet publish "Frontend/Frontend.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Frontend.dll"]