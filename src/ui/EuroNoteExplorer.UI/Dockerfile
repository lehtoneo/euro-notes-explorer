FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /srcroot

ENV DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=0

# Copy entire repository
COPY ../../../ ./

# Restore based on the UI project file only
WORKDIR /srcroot/src/ui/EuroNoteExplorer.UI
RUN dotnet restore

# Publish the UI project
RUN dotnet publish -c Release -o /publish --no-restore /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /publish .
ENTRYPOINT ["dotnet", "EuroNoteExplorer.UI.dll"]
