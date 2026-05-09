# Build and run SpreadsheetApi for Render (and other container hosts).
# Repository root must be the Docker build context so project references resolve.

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source
COPY . .
RUN dotnet publish SpreadsheetApi/SpreadsheetApi.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
# Render sets PORT; bind on all interfaces.
CMD /bin/sh -c "exec dotnet SpreadsheetApi.dll --urls \"http://0.0.0.0:${PORT}\""
