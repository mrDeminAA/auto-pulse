# Build stage
FROM mcr.microsoft.com/dotnet/sdk:11.0-preview AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["src/AutoPulse.Api/AutoPulse.Api.csproj", "src/AutoPulse.Api/"]
COPY ["src/AutoPulse.Infrastructure/AutoPulse.Infrastructure.csproj", "src/AutoPulse.Infrastructure/"]
COPY ["src/AutoPulse.Application/AutoPulse.Application.csproj", "src/AutoPulse.Application/"]
COPY ["src/AutoPulse.Domain/AutoPulse.Domain.csproj", "src/AutoPulse.Domain/"]
RUN dotnet restore "src/AutoPulse.Api/AutoPulse.Api.csproj"

# Copy everything and build
COPY . .
RUN dotnet build "src/AutoPulse.Api/AutoPulse.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "src/AutoPulse.Api/AutoPulse.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:11.0-preview AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Install curl for health checks
RUN apt-get -y update && apt-get -y install curl

COPY --from=publish /app/publish .

# Create user for security
RUN useradd -m appuser && chown -R appuser:appuser /app
USER appuser

ENTRYPOINT ["dotnet", "AutoPulse.Api.dll"]
