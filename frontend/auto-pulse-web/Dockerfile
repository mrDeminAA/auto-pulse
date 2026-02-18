# Этап сборки
FROM node:20-alpine AS build

WORKDIR /app

# Копируем package files
COPY frontend/auto-pulse-web/package*.json ./

# Устанавливаем зависимости
RUN npm install

# Копируем исходный код
COPY frontend/auto-pulse-web/ ./

# Собираем приложение
RUN npm run build -- --configuration=production --base-href=/

# Этап запуска
FROM nginx:alpine

# Копируем собранное приложение
COPY --from=build /app/dist/auto-pulse-web/browser /usr/share/nginx/html

# Копируем конфигурацию nginx
COPY frontend/auto-pulse-web/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
