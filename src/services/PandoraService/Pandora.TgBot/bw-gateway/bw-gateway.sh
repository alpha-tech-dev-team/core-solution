#!/bin/sh

: "${BW_HOST:?Не задан BW_HOST}"
: "${BW_LOGIN:?Не задан BW_LOGIN}"
: "${BW_PASSWORD:?Не задан BW_PASSWORD}"

bw config server "$BW_HOST" > /dev/null 2>&1

echo "Ожидание сервера Vaultwarden: $BW_HOST"

while ! bw login --check > /dev/null 2>&1; do
  if bw login "$BW_LOGIN" "$BW_PASSWORD" --raw > /dev/null 2>&1; then
    break
  fi

  echo "Сервер недоступен, повтор через 10 секунд"
  sleep 10
done

BW_SESSION="$(bw unlock --passwordenv BW_PASSWORD --raw)"

if [ -z "$BW_SESSION" ]; then
  echo "Не удалось разблокировать хранилище"
  exit 1
fi

export BW_SESSION

exec bw serve --hostname 0.0.0.0 --port "${BW_PORT:-8087}" --disable-origin-protection
