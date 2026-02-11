# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy project files for layer caching
COPY RePlay.Domain/*.csproj ./RePlay.Domain/
COPY RePlay.Application/*.csproj ./RePlay.Application/
COPY RePlay.Infrastructure/*.csproj ./RePlay.Infrastructure/
COPY RePlay.API/*.csproj ./RePlay.API/

# Restore dependencies (cached unless .csproj files change)
RUN dotnet restore RePlay.API/RePlay.API.csproj

# Copy source code
COPY . .

# Build the application
RUN dotnet build RePlay.API/RePlay.API.csproj -c Release --no-restore

# Publish the application
RUN dotnet publish RePlay.API/RePlay.API.csproj -c Release -o /app/publish --no-build

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

# Create non-root user for security
RUN groupadd --system appgroup && useradd --system --gid appgroup appuser

# Create uploads directory with proper ownership
RUN mkdir -p /app/uploads/images && chown -R appuser:appgroup /app/uploads

# Copy published application
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

# Switch to non-root user
USER appuser

EXPOSE 5000

ENTRYPOINT ["dotnet", "RePlay.API.dll"]
