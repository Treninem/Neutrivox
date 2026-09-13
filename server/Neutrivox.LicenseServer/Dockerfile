FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore server/Neutrivox.LicenseServer/Neutrivox.LicenseServer.csproj
RUN dotnet publish server/Neutrivox.LicenseServer/Neutrivox.LicenseServer.csproj -c Release -o /out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /out .
ENV ASPNETCORE_URLS=http://+:8080
ENV NEUTRIVOX_DATA_PATH=/data/license-server.json
VOLUME ["/data"]
EXPOSE 8080
ENTRYPOINT ["dotnet", "Neutrivox.LicenseServer.dll"]
