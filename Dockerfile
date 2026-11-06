# 1) Build the React front end
FROM node:22-alpine AS web
WORKDIR /web
COPY web/package*.json ./
RUN if [ -f package-lock.json ]; then npm ci; else npm install; fi
COPY web/ ./
RUN npm run build

# 2) Build the ASP.NET Core API
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/TreatyGraph.Core/*.csproj src/TreatyGraph.Core/
COPY src/TreatyGraph.Api/*.csproj src/TreatyGraph.Api/
RUN dotnet restore src/TreatyGraph.Api/TreatyGraph.Api.csproj
COPY src/ src/
RUN dotnet publish src/TreatyGraph.Api -c Release -o /app --no-restore

# 3) Final image: the API, serving the React build as its web page
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
RUN rm -rf wwwroot/*
COPY --from=web /web/dist ./wwwroot
ENV Urls=http://+:8080
EXPOSE 8080
# Run as the unprivileged "app" user that the .NET image provides, not as root.
USER $APP_UID
ENTRYPOINT ["dotnet", "TreatyGraph.Api.dll"]
