# Multi-stage Dockerfile for the DTD API (.NET 10). Build context = repository root.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the solution and project files first to maximise layer caching.
COPY dtd.slnx ./
COPY src/Dtd.Domain/Dtd.Domain.csproj src/Dtd.Domain/
COPY src/Dtd.Application/Dtd.Application.csproj src/Dtd.Application/
COPY src/Dtd.Infrastructure/Dtd.Infrastructure.csproj src/Dtd.Infrastructure/
COPY src/Dtd.Api/Dtd.Api.csproj src/Dtd.Api/
RUN dotnet restore dtd.slnx

# Copy the rest of the source and publish.
COPY src/ ./src/
RUN dotnet publish src/Dtd.Api/Dtd.Api.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
EXPOSE 8080

# Configuration is supplied through environment variables (see docker-compose / k8s manifests).
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENTRYPOINT ["dotnet", "Dtd.Api.dll"]
