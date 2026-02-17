# Build stage
FROM mcr.microsoft.com/dotnet/sdk:11.0-preview AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["src/AutoPulse.Worker/AutoPulse.Worker.csproj", "src/AutoPulse.Worker/"]
COPY ["src/AutoPulse.Infrastructure/AutoPulse.Infrastructure.csproj", "src/AutoPulse.Infrastructure/"]
COPY ["src/AutoPulse.Application/AutoPulse.Application.csproj", "src/AutoPulse.Application/"]
COPY ["src/AutoPulse.Domain/AutoPulse.Domain.csproj", "src/AutoPulse.Domain/"]
RUN dotnet restore "src/AutoPulse.Worker/AutoPulse.Worker.csproj"

# Copy everything and build
COPY . .
RUN dotnet build "src/AutoPulse.Worker/AutoPulse.Worker.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "src/AutoPulse.Worker/AutoPulse.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/runtime:11.0-preview AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create user for security
RUN useradd -m appuser && chown -R appuser:appuser /app
USER appuser

ENTRYPOINT ["dotnet", "AutoPulse.Worker.dll"]
