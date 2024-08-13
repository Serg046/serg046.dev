FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
COPY . .
RUN dotnet publish -a arm64

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /Server/bin/Release/net8.0/*/publish/ .
EXPOSE 5005
ENTRYPOINT ["./Server.dll"]
