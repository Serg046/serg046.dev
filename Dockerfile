FROM mcr.microsoft.com/dotnet/sdk:8.0.303-bookworm-slim-arm64v8 AS build
COPY . .
RUN dotnet publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0.7-bookworm-slim-arm64v8
WORKDIR /app
COPY --from=build /Server/bin/Release/net8.0/*/publish/ .
EXPOSE 5005
ENTRYPOINT ["./Server.dll"]
