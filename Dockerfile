# Use lightweight Node.js Alpine base image
FROM node:20-alpine

# Create app directory
WORKDIR /usr/src/app

# Install app dependencies
COPY package*.json ./
RUN npm ci --only=production

# Bundle app source
COPY src/ ./src/
COPY knexfile.js ./

# Expose server port
EXPOSE 3000

# Environment defaults
ENV PORT=3000
ENV NODE_ENV=production

# Run command
CMD [ "npm", "start" ]
