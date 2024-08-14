FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim-arm64v8 AS build
COPY . .
RUN dotnet publish Server/Server.csproj -a arm64

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim-arm64v8
WORKDIR /app
COPY --from=build /Server/bin/Release/net8.0/*/publish/ .
RUN chmod +x Server.dll
EXPOSE 5005
ENTRYPOINT ["./Server.dll"]
