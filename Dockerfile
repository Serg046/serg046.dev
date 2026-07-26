FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
COPY . .
RUN dotnet restore Server/Server.csproj
RUN dotnet publish Server/Server.csproj --no-restore

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /Server/bin/Release/net10.0/publish/ .
EXPOSE 8080
ENTRYPOINT ["dotnet", "./Server.dll"]
