FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app
COPY /Client/Client.csproj /app/Client/Client.csproj
COPY /Server/Server.csproj /app/Server/Server.csproj
RUN dotnet restore /app/Client/Client.csproj && dotnet restore /app/Server/Server.csproj
COPY . .
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "Server.dll"]