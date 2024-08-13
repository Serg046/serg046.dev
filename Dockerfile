FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine3.19-arm64v8 AS build
RUN dotnet workload install wasm-tools
COPY . .
RUN dotnet publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /Server/bin/Release/net8.0/*/publish/ .
EXPOSE 5005
ENTRYPOINT ["./Server.dll"]
