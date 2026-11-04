#!/bin/sh

BW_PASSWORD_FILE="/run/secrets/bw_password"

: "${BW_HOST:?Не задан BW_HOST}"
: "${BW_LOGIN:?Не задан BW_LOGIN}"

if [ ! -r "$BW_PASSWORD_FILE" ]; then
  echo "Недоступен файл пароля шлюза: $BW_PASSWORD_FILE"
  exit 1
fi

bw config server "$BW_HOST" > /dev/null 2>&1

echo "Ожидание сервера Vaultwarden: $BW_HOST"

while ! bw login --check > /dev/null 2>&1; do
  if bw login "$BW_LOGIN" --passwordfile "$BW_PASSWORD_FILE" --raw > /dev/null 2>&1; then
    break
  fi

  echo "Сервер недоступен, повтор через 10 секунд"
  sleep 10
done

BW_SESSION="$(bw unlock --passwordfile "$BW_PASSWORD_FILE" --raw)"

if [ -z "$BW_SESSION" ]; then
  echo "Не удалось разблокировать хранилище"
  exit 1
fi

export BW_SESSION

exec bw serve --hostname 0.0.0.0 --port "${BW_PORT:-8087}" --disable-origin-protection
