# StackFlow API — Multi-stage Dockerfile
#
# Stage 1 (build): uses the full .NET SDK image to restore, build, and publish the app.
# Stage 2 (runtime): copies only the published output into the smaller ASP.NET runtime image.
#
# Why multi-stage: the SDK image is ~800 MB. The runtime image is ~200 MB.
# The final image contains no build tools — smaller attack surface, faster pulls.
#
# Build context is ./web-api (set in docker-compose.yml).
# The solution and all project files are relative to that context.

# ── Stage 1: Build ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

# Copy solution file and all project files first.
# This layer is cached as long as .csproj files don't change —
# restoring NuGet packages won't re-run unless dependencies change.
COPY StackFlow.sln ./
COPY src/StackFlow.Domain/StackFlow.Domain.csproj             src/StackFlow.Domain/
COPY src/StackFlow.Application/StackFlow.Application.csproj   src/StackFlow.Application/
COPY src/StackFlow.Infrastructure/StackFlow.Infrastructure.csproj src/StackFlow.Infrastructure/
COPY src/StackFlow.Api/StackFlow.Api.csproj                   src/StackFlow.Api/
COPY tests/StackFlow.UnitTests/StackFlow.UnitTests.csproj     tests/StackFlow.UnitTests/
COPY tests/StackFlow.IntegrationTests/StackFlow.IntegrationTests.csproj tests/StackFlow.IntegrationTests/

# Restore all packages in a single layer (cached until .csproj files change)
RUN dotnet restore StackFlow.sln

# Copy the rest of the source code
COPY . .

# Publish the API project in Release configuration
# Output goes to /app/publish — no debug symbols, no runtime identifiers needed
# (the base image provides the .NET runtime)
RUN dotnet publish src/StackFlow.Api/StackFlow.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

# ── Stage 2: Runtime ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Copy published output from the build stage only — no SDK, no source code
COPY --from=build /app/publish .

# Port 8080 is the ASP.NET Core default in .NET 8+
# docker-compose maps 5000:8080 so the host reaches the API at localhost:5000
EXPOSE 8080

# Set to Production by default; docker-compose overrides to Development for local dev
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "StackFlow.Api.dll"]
