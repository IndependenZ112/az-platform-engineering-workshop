FROM mcr.microsoft.com/azurelinux/base/nodejs:24 AS build
WORKDIR /app

ARG NPM_REGISTRY=https://registry.npmjs.org/

COPY package.json package-lock.json ./
RUN --mount=type=cache,target=/root/.npm \
    npm ci --cache /root/.npm --registry "$NPM_REGISTRY"

COPY . .
RUN npm run build

FROM mcr.microsoft.com/azurelinux/base/nginx:1.28 AS runtime

COPY --chown=nginx:nginx nginx/nginx.conf /etc/nginx/nginx.conf
COPY --chown=nginx:nginx nginx/default.conf.template /etc/nginx/templates/default.conf.template
COPY --chown=nginx:nginx nginx/entrypoint.sh /usr/local/bin/frontend-entrypoint.sh
COPY --from=build --chown=nginx:nginx /app/dist /usr/share/nginx/html

RUN chmod 0555 /usr/local/bin/frontend-entrypoint.sh \
    && chown nginx:nginx /var/log/nginx

USER nginx
EXPOSE 8080

ENTRYPOINT ["/usr/local/bin/frontend-entrypoint.sh"]
CMD ["nginx", "-g", "daemon off;"]
