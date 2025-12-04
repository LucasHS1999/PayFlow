# FastPay.Mock Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ./src/FastPay.Mock/FastPay.Mock.csproj ./src/FastPay.Mock/
RUN dotnet restore "./src/FastPay.Mock/FastPay.Mock.csproj"
COPY ./src/FastPay.Mock ./src/FastPay.Mock
RUN dotnet publish "./src/FastPay.Mock/FastPay.Mock.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FastPay.Mock.dll"]
