FROM mcr.microsoft.com/dotnet/aspnet:6.0-focal AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0-focal AS build
RUN sudo apt-get update
RUN sudo apt-get install dos2unix --yes
WORKDIR /src
COPY /Client/Client.csproj /app/Client/Client.csproj
COPY /Server/Server.csproj /app/Server/Server.csproj
RUN dotnet restore /app/Client/Client.csproj && dotnet restore /app/Server/Server.csproj
COPY . .
RUN dos2unix $(find . -type f)
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Server.dll"]
