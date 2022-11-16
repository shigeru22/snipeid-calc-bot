# syntax=docker/dockerfile:1

FROM node:16

ENV NODE_ENV=production

WORKDIR /app
COPY package*.json ./
RUN npm ci --include=dev
COPY . ./
RUN npm run build
RUN npm prune --production

CMD [ "npm", "start" ]
