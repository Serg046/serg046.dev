FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG TARGETARCH
COPY Server/Server.csproj Server/Server.csproj
COPY Client/Client.csproj Client/Client.csproj
RUN dotnet restore Server/Server.csproj -a $TARGETARCH && dotnet restore Client/Client.csproj -a $TARGETARCH
COPY . .
RUN dotnet publish Server/Server.csproj -a $TARGETARCH --no-restore

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim-arm64v8
WORKDIR /app
COPY --from=build /Server/bin/Release/net8.0/*/publish/ .
EXPOSE 8080
ENTRYPOINT ["dotnet", "./Server.dll"]
