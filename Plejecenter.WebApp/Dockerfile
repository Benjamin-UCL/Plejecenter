FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# Copy csproj files first for restore-layer caching
COPY ["Plejecenter.WebApp/Plejecenter.WebApp.csproj", "Plejecenter.WebApp/"]
COPY ["Plejecenter.Domain/Plejecenter.Domain.csproj", "Plejecenter.Domain/"]
COPY ["Plejecenter.Shared/Plejecenter.Shared.csproj", "Plejecenter.Shared/"]
RUN dotnet restore "Plejecenter.WebApp/Plejecenter.WebApp.csproj"
COPY . .
RUN dotnet publish "Plejecenter.WebApp/Plejecenter.WebApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080 
ENTRYPOINT ["dotnet", "Plejecenter.WebApp.dll"]