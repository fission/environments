# Use the .NET 8 SDK image for runtime (needed for compilation support)
FROM mcr.microsoft.com/dotnet/sdk:8.0

# Install required tools
RUN apt-get update && apt-get install -y unzip curl && rm -rf /var/lib/apt/lists/*

# Set working directory
WORKDIR /app

# Copy the Fission .NET runtime application
COPY Fission.DotNet/ /app/Fission.DotNet/
COPY Fission.DotNet.Common/ /app/Fission.DotNet.Common/

# Build the runtime application
RUN dotnet restore Fission.DotNet/Fission.DotNet.csproj
RUN dotnet publish Fission.DotNet/Fission.DotNet.csproj -c Release -o /app/publish

# Build and copy Fission.DotNet.Common.dll to /app for runtime compilation
RUN dotnet build Fission.DotNet.Common/Fission.DotNet.Common.csproj -c Release -o /app/

# Create function directory
RUN mkdir -p /function

# Set environment variables
ENV ASPNETCORE_URLS=http://*:8888
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port
EXPOSE 8888

# Start the runtime
CMD ["dotnet", "/app/publish/Fission.DotNet.dll"]
