# Prep work for all images.
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 8888

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS restore
WORKDIR /src
COPY . /src
RUN dotnet restore "./fission-dotnet6.sln"

# Specific work for environment image.
FROM restore as build-env
RUN dotnet publish "./fission-dotnet6/fission-dotnet6.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build-env /app/publish .
ENTRYPOINT ["dotnet", "fission-dotnet6.dll"]
