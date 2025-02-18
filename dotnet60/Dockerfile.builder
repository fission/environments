ARG BUILDER_IMAGE=fission/builder
FROM ${BUILDER_IMAGE} AS fission-builder


FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builderimage


WORKDIR /app

# Copy csproj and restore as distinct layers
COPY Common ./../Common
COPY Fission.Functions ./../Fission.Functions
COPY builder/*.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY builder ./
RUN dotnet publish -c Release -o out


# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=builderimage /app/out .

# this builder is actually compilation from : https://github.com/fission/fission/tree/master/builder/cmd  and renamed cmd.exe to builder
# make sure to compile it in linux only else you will get exec execute error as binary was compiled in windows and running on linux

COPY --from=fission-builder /builder /builder

#ADD builder /builder

ADD builder/build.sh /usr/local/bin/build
RUN chmod +x /usr/local/bin/build

ADD builder/build.sh /bin/build
RUN chmod +x /bin/build

EXPOSE 8001