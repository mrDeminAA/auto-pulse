# Stage 1: Build
FROM node:20-alpine AS build

WORKDIR /app

# Copy package files only
COPY package*.json ./

# Install dependencies (fresh for Linux platform)
RUN npm ci

# Copy source code (excluding node_modules via .dockerignore)
COPY . .

# Build the application
RUN npm run build -- --configuration production

# Stage 2: Serve with Nginx
FROM nginx:alpine

# Copy custom nginx config
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy built application from build stage
COPY --from=build /app/dist/auto-pulse-web/browser /usr/share/nginx/html

# Expose port 80
EXPOSE 80

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --quiet --tries=1 --spider http://localhost/ || exit 1

# Start nginx
CMD ["nginx", "-g", "daemon off;"]
