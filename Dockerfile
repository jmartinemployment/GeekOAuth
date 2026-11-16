FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore GeekOAuth.slnx \
    && dotnet publish src/GeekOAuth.Server/GeekOAuth.Server.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
# Run as the image's built-in non-root "app" user (uid 1654); keep the Serilog file-sink dir writable.
RUN mkdir -p /app/logs && chown -R app:app /app/logs
USER app
ENTRYPOINT ["dotnet", "GeekOAuth.Server.dll"]
