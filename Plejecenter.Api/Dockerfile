FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# Copy csproj files first for restore-layer caching
COPY ["Plejecenter.Api/Plejecenter.Api.csproj", "Plejecenter.Api/"]
COPY ["Plejecenter.Domain/Plejecenter.Domain.csproj", "Plejecenter.Domain/"]
COPY ["Plejecenter.Application/Plejecenter.Application.csproj", "Plejecenter.Application/"]
COPY ["Plejecenter.Infrastructure/Plejecenter.Infrastructure.csproj", "Plejecenter.Infrastructure/"]
COPY ["Plejecenter.Shared/Plejecenter.Shared.csproj", "Plejecenter.Shared/"]
RUN dotnet restore "Plejecenter.Api/Plejecenter.Api.csproj"
COPY . .
RUN dotnet publish "Plejecenter.Api/Plejecenter.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Plejecenter.Api.dll"]