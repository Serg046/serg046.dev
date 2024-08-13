FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-alpine-arm64v8 AS build
ARG TARGETARCH
RUN dotnet workload install wasm-tools
COPY . .
RUN dotnet publish serg046.dev.slnx -a $TARGETARCH

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /Server/bin/Release/net8.0/*/publish/ .
EXPOSE 5005
ENTRYPOINT ["./Server.dll"]
