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

# Install dependencies for Playwright
RUN apt-get update && apt-get install -y \
    libnss3 \
    libnspr4 \
    libatk1.0-0 \
    libatk-bridge2.0-0 \
    libcups2 \
    libdrm2 \
    libxkbcommon0 \
    libxcomposite1 \
    libxdamage1 \
    libxfixes3 \
    libxrandr2 \
    libgbm1 \
    libasound2 \
    libpango-1.0-0 \
    libcairo2 \
    libatspi2.0-0 \
    --no-install-recommends

WORKDIR /app
COPY --from=publish /app/publish .

# Install Playwright browsers
RUN pwsh bin/Debug/netX/playwright.ps1 install chromium

# Create user for security
RUN useradd -m appuser && chown -R appuser:appuser /app
USER appuser

ENTRYPOINT ["dotnet", "AutoPulse.Worker.dll"]
