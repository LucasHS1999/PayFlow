# PayFlow.Api Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ./src/PayFlow.Api/PayFlow.Api.csproj ./src/PayFlow.Api/
RUN dotnet restore "./src/PayFlow.Api/PayFlow.Api.csproj"
COPY ./src/PayFlow.Api ./src/PayFlow.Api
RUN dotnet publish "./src/PayFlow.Api/PayFlow.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PayFlow.Api.dll"]
