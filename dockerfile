# Используем .NET 9 SDK
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Копируем csproj и устанавливаем зависимости
COPY *.sln .
COPY JobBackend/*.csproj ./JobBackend/
COPY JobBackend.Application/*.csproj ./JobBackend.Application/
COPY JobBackend.Domain/*.csproj ./JobBackend.Domain/
COPY JobBackend.Infrastructure/*.csproj ./JobBackend.Infrastructure/
RUN dotnet restore

# Копируем остальной код и собираем
COPY . .
RUN dotnet publish JobBackend/JobBackend.csproj -c Release -o /out

# Финальный образ
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "JobBackend.dll"]