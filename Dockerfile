FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG TARGETARCH
COPY . .
RUN cat Server/appsettings.json
RUN dotnet publish Server/Server.csproj -a $TARGETARCH
RUN cat appsettings.json

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim-arm64v8
WORKDIR /app
COPY --from=build /Server/bin/Release/net8.0/*/publish/ .
EXPOSE 8080
ENTRYPOINT ["dotnet", "./Server.dll"]
