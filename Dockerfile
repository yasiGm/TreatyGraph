FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/TreatyGraph.Core/*.csproj src/TreatyGraph.Core/
COPY src/TreatyGraph.Api/*.csproj src/TreatyGraph.Api/
RUN dotnet restore src/TreatyGraph.Api/TreatyGraph.Api.csproj
COPY src/ src/
RUN dotnet publish src/TreatyGraph.Api -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENV Urls=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "TreatyGraph.Api.dll"]