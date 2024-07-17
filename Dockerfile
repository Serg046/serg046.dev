FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG TARGETARCH
WORKDIR /src
COPY /Client/Client.csproj /app/Client/Client.csproj
COPY /Server/Server.csproj /app/Server/Server.csproj
RUN dotnet restore
COPY . .
RUN dotnet publish --no-restore -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 5005
ENTRYPOINT ["./Server.dll"]
