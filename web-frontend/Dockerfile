# StackFlow Frontend — Multi-stage Dockerfile
#
# Stage 1 (build): Node image installs dependencies and runs `vite build`.
# Stage 2 (runtime): Nginx alpine serves the compiled dist/ output.
#
# Why multi-stage: the Node build image includes the full npm toolchain (~1 GB).
# The final Nginx image is ~25 MB. Source code and node_modules are not in production.
#
# Build context is ./web-frontend (set in docker-compose.yml).
# Expose port 80 — docker-compose maps 3000:80 so host reaches the frontend at localhost:3000.

# ── Stage 1: Build ────────────────────────────────────────────────────────────
FROM node:20-alpine AS build
WORKDIR /app

# Copy package files first — this layer is cached until package.json changes.
# npm ci installs exact versions from package-lock.json (faster, reproducible).
COPY package.json package-lock.json ./
RUN npm ci

# Copy source after installing deps — avoids re-running npm ci on every source change
COPY . .

# Build the Vite React app — output goes to /app/dist
# VITE_API_URL is a build-time variable injected via docker-compose environment
ARG VITE_API_URL
ENV VITE_API_URL=${VITE_API_URL}
RUN npm run build

# ── Stage 2: Runtime ─────────────────────────────────────────────────────────
FROM nginx:alpine AS runtime

# Copy the Vite build output into Nginx's default serve directory
COPY --from=build /app/dist /usr/share/nginx/html

# Copy custom Nginx config to handle React Router (client-side routing).
# Without this, refreshing any route other than "/" returns 404 from Nginx.
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
