FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5005

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/NotificationService.Api/NotificationService.Api.csproj", "src/NotificationService.Api/"]
COPY ["src/NotificationService.Core/NotificationService.Core.csproj", "src/NotificationService.Core/"]
COPY ["src/NotificationService.Infrastructure/NotificationService.Infrastructure.csproj", "src/NotificationService.Infrastructure/"]
RUN dotnet restore "src/NotificationService.Api/NotificationService.Api.csproj"
COPY . .
RUN dotnet build "src/NotificationService.Api/NotificationService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/NotificationService.Api/NotificationService.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NotificationService.Api.dll"]
