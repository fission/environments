ARG BUILDER_IMAGE=ghcr.io/fission/builder:v1.21.0
ARG DOTNET_BASE_IMG=mcr.microsoft.com/dotnet/sdk:8.0

FROM ${BUILDER_IMAGE}
FROM ${DOTNET_BASE_IMG}

COPY --from=0 /builder /builder

# Install required tools
RUN apt-get update && apt-get install -y unzip zip curl && rm -rf /var/lib/apt/lists/*

# Create Fission.DotNet.Common project directory
RUN mkdir -p /app/Fission.DotNet.Common

# Copy Fission.DotNet.Common source files
COPY Fission.DotNet.Common/ /app/Fission.DotNet.Common/

# Build the Fission.DotNet.Common library properly
WORKDIR /app/Fission.DotNet.Common
RUN dotnet build -c Release
RUN dotnet publish -c Release -o /app

# Verify the DLL was created
RUN ls -la /app/Fission.DotNet.Common.dll

# Copy the build script
COPY builder/defaultBuildCmd /usr/local/bin/build
COPY builder/defaultBuildCmd /build
RUN chmod +x /usr/local/bin/build /build

EXPOSE 8001 