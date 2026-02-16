FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["FraudDetection.Api/FraudDetection.Api.csproj", "FraudDetection.Api/"]
COPY ["FraudDetection.Core/FraudDetection.Core.csproj", "FraudDetection.Core/"]
COPY ["FraudDetection.Infrastructure/FraudDetection.Infrastructure.csproj", "FraudDetection.Infrastructure/"]
RUN dotnet restore "FraudDetection.Api/FraudDetection.Api.csproj"
COPY . .
WORKDIR "/src/FraudDetection.Api"
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FraudDetection.Api.dll"]