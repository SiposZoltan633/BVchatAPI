FROM mcr.microsoft.com/dotnet/aspnet:8.0
AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS
build
WORKDIR /rc

COPY . .

RUN dotnet restore ./BVchatAPI.csproj
RUN dotnet publish ./BVchatAPI.csproj -c
Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "BVchatAPI.ddl"]