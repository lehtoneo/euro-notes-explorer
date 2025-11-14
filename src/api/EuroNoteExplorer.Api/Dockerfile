FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /srcroot

ENV DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=0

# Copy entire repository into the container
COPY ../../../ ./

# Restore based on the API project file only
WORKDIR /srcroot/src/api/EuroNoteExplorer.Api
RUN dotnet restore

# Publish the API project
RUN dotnet publish -c Release -o /publish --no-restore /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /publish .
ENTRYPOINT ["dotnet", "EuroNoteExplorer.Api.dll"]
