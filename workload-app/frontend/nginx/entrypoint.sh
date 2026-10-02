#!/bin/sh
set -eu

if ! printf '%s' "${BACKEND_URL:-}" | grep -Eq '^https?://[A-Za-z0-9.-]+(:[0-9]+)?$'; then
    echo "BACKEND_URL must be an HTTP(S) URL containing only a host and optional port." >&2
    exit 1
fi

sed "s|__BACKEND_URL__|${BACKEND_URL}|g" \
    /etc/nginx/templates/default.conf.template > /tmp/default.conf

exec "$@"
