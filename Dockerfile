FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/MeloSupportDesk.Api/MeloSupportDesk.Api.csproj src/MeloSupportDesk.Api/
RUN dotnet restore src/MeloSupportDesk.Api/MeloSupportDesk.Api.csproj

COPY . .
RUN dotnet publish src/MeloSupportDesk.Api/MeloSupportDesk.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

CMD ["sh", "-c", "dotnet MeloSupportDesk.Api.dll --urls http://0.0.0.0:${PORT:-8080}"]
