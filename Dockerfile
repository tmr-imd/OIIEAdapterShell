FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
RUN apt-get -y update \
    && apt-get install -y curl \
    && curl -fsSL https://deb.nodesource.com/setup_18.x | bash - \
    && apt-get install -y nodejs \
    && apt-get clean \
    && echo 'node verions:' $(node -v) \
    && echo 'npm version:' $(npm -v)

WORKDIR /src
COPY ["src/AdapterServer/AdapterServer.csproj", "src/AdapterServer/AdapterServer.csproj"]
RUN dotnet restore --use-current-runtime "src/AdapterServer/AdapterServer.csproj"

# use `docker build --build-arg configuration=Debug --build-arg environment=Development ...` for a development build
ARG configuration
ARG environment
ENV BUILD_CONFIGURATION=${configuration:-Release}
ENV ASPNETCORE_ENVIRONMENT=${environment:-Production}

COPY . .
WORKDIR "/src/src/AdapterServer"
RUN dotnet build "AdapterServer.csproj" -c ${BUILD_CONFIGURATION} -o /app/build

FROM build AS publish
RUN dotnet publish "AdapterServer.csproj" -c ${BUILD_CONFIGURATION} -o /app/publish /p:UseAppHost=false
# Include only environment specific appsettings if present.
RUN ( ls /app/appsettings.${ASPNETCORE_ENVIRONMENT}.json && \
    rm /app/appsettings.json && \
    mv /app/appsettings.${ASPNETCORE_ENVIRONMENT}.json /app/appsettings.json ) || \
    true

FROM base AS final
LABEL org.opencontainers.image.source https://github.com/tmr-imd/OIIEAdapterShell
WORKDIR /app
COPY --from=publish --chown=appuser /app/publish .
RUN find /app -type d -exec chmod 750 {} + ; \
    find /app -type f -exec chmod 640 {} + ;
VOLUME [ "/app/Data" ]
EXPOSE 80
ENTRYPOINT ["dotnet", "AdapterServer.dll"]
