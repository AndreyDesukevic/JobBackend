FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY ./src/JobBackend.API/*.csproj ./src/JobBackend.API/
COPY ./src/JobBackend.Application/*.csproj ./src/JobBackend.Application/
COPY ./src/JobBackend.Domain/*.csproj ./src/JobBackend.Domain/
COPY ./src/JobBackend.Infrastructure/*.csproj ./src/JobBackend.Infrastructure/

RUN dotnet restore ./src/JobBackend.API/JobBackend.API.csproj

COPY ./src ./src
WORKDIR /app/src/JobBackend.API
RUN dotnet publish -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /publish .
COPY ./secrets/secret.json ./secrets/secret.json
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000
ENTRYPOINT ["dotnet", "JobBackend.API.dll"]