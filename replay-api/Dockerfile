# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy solution and project files
COPY RePlay.API.sln .
COPY RePlay.Domain/*.csproj ./RePlay.Domain/
COPY RePlay.Application/*.csproj ./RePlay.Application/
COPY RePlay.Infrastructure/*.csproj ./RePlay.Infrastructure/
COPY RePlay.API/*.csproj ./RePlay.API/

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY . .

# Build the application
RUN dotnet build -c Release --no-restore

# Publish the application
RUN dotnet publish RePlay.API/RePlay.API.csproj -c Release -o /app/publish --no-build

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

# Create uploads directory
RUN mkdir -p /app/uploads/images

# Copy published application
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 5000

ENTRYPOINT ["dotnet", "RePlay.API.dll"]
