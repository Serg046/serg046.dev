FROM --platform=linux/arm64 mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG TARGETARCH
WORKDIR /src
COPY *.csproj .
RUN dotnet restore -a $TARGETARCH
COPY . .
RUN dotnet publish -a $TARGETARCH --no-restore -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 5005
ENTRYPOINT ["./Server.dll"]
