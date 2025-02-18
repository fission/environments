# Use the ASP.NET Core 8 base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Use the .NET 8 SDK image for the build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .

RUN dotnet restore Fission.DotNet/Fission.DotNet.csproj

RUN dotnet publish Fission.DotNet/Fission.DotNet.csproj -c Release -o /app

# Final image
FROM base AS final
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Fission.DotNet.dll"]
