FROM node:20.12
WORKDIR /app
COPY ["web/package.json", "web/package-lock.json", "web/"]
WORKDIR /app/web
RUN npm ci --legacy-peer-deps
COPY . ..
ENV VITE_INTERNAL_API_URL http://planera-api:5065
ENV VITE_PUBLIC_API_URL /api
RUN npm run build
ENV PORT 3000
EXPOSE 3000
CMD ["node", "build"]
