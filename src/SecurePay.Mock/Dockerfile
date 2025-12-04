# SecurePay.Mock Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ./src/SecurePay.Mock/SecurePay.Mock.csproj ./src/SecurePay.Mock/
RUN dotnet restore "./src/SecurePay.Mock/SecurePay.Mock.csproj"
COPY ./src/SecurePay.Mock ./src/SecurePay.Mock
RUN dotnet publish "./src/SecurePay.Mock/SecurePay.Mock.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SecurePay.Mock.dll"]
