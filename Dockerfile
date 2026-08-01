# Stage 1: Build React Frontend
FROM node:22-alpine AS frontend-builder
WORKDIR /src
COPY frontend/package*.json ./
RUN npm install
COPY frontend/ ./
RUN npm run build

# Stage 2: Build C# Backend
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS backend-builder
WORKDIR /src
COPY backend/backend.csproj ./
RUN dotnet restore
COPY backend/ ./
# Copy React static assets to C# wwwroot folder
COPY --from=frontend-builder /src/dist ./wwwroot
RUN dotnet publish -c Release -o /app/publish

# Stage 3: Run Monolith App
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=backend-builder /app/publish .
EXPOSE 3000
ENV PORT=3000
ENV ASPNETCORE_URLS=http://+:3000
ENTRYPOINT ["dotnet", "backend.dll"]
